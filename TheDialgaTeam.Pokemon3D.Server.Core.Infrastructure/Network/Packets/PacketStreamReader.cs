// Pokemon 3D Server Client
// Copyright (C) 2026 Yong Jian Ming
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Infrastructure.Network.Packets;

public class PacketStreamReader(Stream stream, int bufferSize)
{
    private record ReadResult<T>(
        [property: MemberNotNullWhen(true, "Value")]
        bool Success,
        T? Value
    ) where T : ISpanParsable<T>;

    private readonly StreamReader _reader = new(stream, Encoding.UTF8, false, bufferSize, true);
    private readonly Memory<char> _charBufferMemory = new(new char[Encoding.UTF8.GetMaxCharCount(bufferSize)]);
    private int _position;
    private int _length;
    private bool _isErrorState;

    public async Task<IRawPacket?> ReadPacketAsync(CancellationToken token = default)
    {
        if (_isErrorState) return null;

        var readVersionResult = await ReadUntilNextTokenAsync<string>(32, token).ConfigureAwait(false);

        if (!readVersionResult.Success)
        {
            _isErrorState = true;
            return null;
        }

        var readPacketTypeResult = await ReadUntilNextTokenAsync<int>(10, token).ConfigureAwait(false);

        if (!readPacketTypeResult.Success)
        {
            _isErrorState = true;
            return null;
        }

        var packetType = PacketType.Unknown;

        if (Enum.IsDefined(typeof(PacketType), readPacketTypeResult.Value))
        {
            packetType = (PacketType) readPacketTypeResult.Value;
        }

        var readOriginResult = await ReadUntilNextTokenAsync<int>(10, token).ConfigureAwait(false);

        if (!readOriginResult.Success)
        {
            _isErrorState = true;
            return null;
        }

        var readDataItemCountResult = await ReadUntilNextTokenAsync<int>(10, token).ConfigureAwait(false);

        if (!readDataItemCountResult.Success)
        {
            _isErrorState = true;
            return null;
        }

        if (readDataItemCountResult.Value == 0)
        {
            var extraResult = await ReadUntilEndOfLineAsync<string>(0, token).ConfigureAwait(false);

            if (!extraResult.Success)
            {
                _isErrorState = true;
                return null;
            }

            return new RawPacket(readVersionResult.Value, packetType, readOriginResult.Value, []);
        }

        var dataItemIndexes = ArrayPool<int>.Shared.Rent(readDataItemCountResult.Value);

        try
        {
            for (var i = 0; i < readDataItemCountResult.Value; i++)
            {
                var readIndexResult = await ReadUntilNextTokenAsync<int>(10, token).ConfigureAwait(false);

                if (!readIndexResult.Success)
                {
                    _isErrorState = true;
                    return null;
                }

                dataItemIndexes[i] = readIndexResult.Value;
            }

            var dataItems = new string[readDataItemCountResult.Value];

            for (var i = 0; i < readDataItemCountResult.Value - 1; i++)
            {
                var dataItemResult = await ReadExactAsync<string>(dataItemIndexes[i + 1] - dataItemIndexes[i], token).ConfigureAwait(false);

                if (!dataItemResult.Success)
                {
                    _isErrorState = true;
                    return null;
                }

                dataItems[i] = dataItemResult.Value;
            }

            var readFinalResult = await ReadUntilEndOfLineAsync<string>(1 * 1024 * 1024, token).ConfigureAwait(false);

            if (!readFinalResult.Success)
            {
                _isErrorState = true;
                return null;
            }

            dataItems[^1] = readFinalResult.Value;

            return new RawPacket(readVersionResult.Value, packetType, readOriginResult.Value, dataItems);
        }
        finally
        {
            ArrayPool<int>.Shared.Return(dataItemIndexes);
        }
    }

    private async Task<int> ReadToBufferAsync(CancellationToken token = default)
    {
        var read = await _reader.ReadAsync(_charBufferMemory, token).ConfigureAwait(false);

        _position = 0;
        _length = read;

        return read;
    }

    private async Task<ReadResult<T>> ReadUntilNextTokenAsync<T>(int maxLength, CancellationToken token = default) where T : ISpanParsable<T>
    {
        using var resultCharBufferMemoryPool = MemoryPool<char>.Shared.Rent(maxLength);
        var resultCharBufferMemory = resultCharBufferMemoryPool.Memory;
        var resultCharBufferLength = 0;

        do
        {
            // If the current internal buffer isn't empty.
            if (_position < _length)
            {
                var spaceRemaining = maxLength - resultCharBufferLength;
                var currentCharBufferMemory = _charBufferMemory.Slice(_position, _length - _position);
                var currentCharBufferSpan = currentCharBufferMemory.Span;

                var searchIndex = currentCharBufferSpan.IndexOfAny('|', '\r', '\n');

                if (searchIndex != -1)
                {
                    switch (currentCharBufferSpan[searchIndex])
                    {
                        // Found stop token
                        case '\r':
                        {
                            if (searchIndex + 1 < currentCharBufferSpan.Length && currentCharBufferSpan[searchIndex + 1] == '\n')
                            {
                                _position += searchIndex + 2;
                            }
                            else
                            {
                                if (await ReadToBufferAsync(token).ConfigureAwait(false) == 0)
                                {
                                    return new ReadResult<T>(false, default);
                                }

                                if (currentCharBufferMemory.Span[0] == '\n')
                                {
                                    _position += 1;
                                }
                            }

                            return new ReadResult<T>(false, default);
                        }

                        case '\n':
                        {
                            _position += searchIndex + 1;
                            return new ReadResult<T>(false, default);
                        }

                        case '|':
                        {
                            _position += searchIndex + 1;
                            break;
                        }
                    }

                    if (searchIndex > spaceRemaining)
                    {
                        return new ReadResult<T>(false, default);
                    }

                    currentCharBufferMemory[..searchIndex].CopyTo(resultCharBufferMemory.Slice(resultCharBufferLength, searchIndex));
                    resultCharBufferLength += searchIndex;
                    break;
                }

                // Not found so let append into the buffer
                if (resultCharBufferLength + currentCharBufferSpan.Length <= maxLength)
                {
                    currentCharBufferSpan.CopyTo(resultCharBufferMemory.Slice(resultCharBufferLength, currentCharBufferSpan.Length).Span);
                    resultCharBufferLength += currentCharBufferSpan.Length;
                }

                _position += currentCharBufferSpan.Length;
            }

            // Read More
            if (await ReadToBufferAsync(token).ConfigureAwait(false) == 0)
            {
                return new ReadResult<T>(false, default);
            }
        } while (true);

        return T.TryParse(resultCharBufferMemory[..resultCharBufferLength].Span, CultureInfo.InvariantCulture, out var result)
            ? new ReadResult<T>(true, result)
            : new ReadResult<T>(false, default);
    }

    private async Task<ReadResult<T>> ReadExactAsync<T>(int length, CancellationToken token = default) where T : ISpanParsable<T>
    {
        using var resultCharBufferMemoryPool = MemoryPool<char>.Shared.Rent(length);
        var resultCharBufferMemory = resultCharBufferMemoryPool.Memory;
        var resultCharBufferLength = 0;

        do
        {
            // If the current internal buffer isn't empty.
            if (_position < _length)
            {
                var spaceRemaining = length - resultCharBufferLength;
                var currentCharBufferMemory = _charBufferMemory.Slice(_position, Math.Min(_length - _position, spaceRemaining));
                var currentCharBufferSpan = currentCharBufferMemory.Span;

                var searchIndex = currentCharBufferSpan.IndexOfAny('\r', '\n');

                if (searchIndex != -1)
                {
                    // Found stop token within the target length so this is bad
                    if (currentCharBufferSpan[searchIndex] == '\r')
                    {
                        if (searchIndex + 1 < currentCharBufferSpan.Length && currentCharBufferSpan[searchIndex + 1] == '\n')
                        {
                            _position += searchIndex + 2;
                        }
                        else
                        {
                            if (await ReadToBufferAsync(token).ConfigureAwait(false) == 0)
                            {
                                return new ReadResult<T>(false, default);
                            }

                            if (currentCharBufferMemory.Span[0] == '\n')
                            {
                                _position += 1;
                            }
                        }
                    }
                    else
                    {
                        _position += searchIndex + 1;
                    }

                    return new ReadResult<T>(false, default);
                }

                // Not found so let append into the buffer
                currentCharBufferSpan.CopyTo(resultCharBufferMemory.Slice(resultCharBufferLength, currentCharBufferSpan.Length).Span);
                resultCharBufferLength += currentCharBufferSpan.Length;

                _position += currentCharBufferSpan.Length;
            }

            if (resultCharBufferLength == length)
            {
                // Reached the target length so we don't need to repeat the reading again.
                break;
            }

            // Read More
            if (await ReadToBufferAsync(token).ConfigureAwait(false) == 0)
            {
                return new ReadResult<T>(false, default);
            }
        } while (resultCharBufferLength < length);

        return T.TryParse(resultCharBufferMemory[..resultCharBufferLength].Span, CultureInfo.InvariantCulture, out var result)
            ? new ReadResult<T>(true, result)
            : new ReadResult<T>(false, default);
    }

    private async Task<ReadResult<T>> ReadUntilEndOfLineAsync<T>(int maxLength, CancellationToken token) where T : ISpanParsable<T>
    {
        using var resultCharBufferMemoryPool = MemoryPool<char>.Shared.Rent(maxLength);
        var resultCharBufferMemory = resultCharBufferMemoryPool.Memory;
        var resultCharBufferLength = 0;

        do
        {
            // If the current internal buffer isn't empty.
            if (_position < _length)
            {
                var spaceRemaining = maxLength - resultCharBufferLength;
                var currentCharBufferMemory = _charBufferMemory.Slice(_position, _length - _position);
                var currentCharBufferSpan = currentCharBufferMemory.Span;

                var searchIndex = currentCharBufferSpan.IndexOfAny('\r', '\n');

                if (searchIndex != -1)
                {
                    // Found stop token
                    if (currentCharBufferSpan[searchIndex] == '\r')
                    {
                        if (searchIndex + 1 < currentCharBufferSpan.Length && currentCharBufferSpan[searchIndex + 1] == '\n')
                        {
                            _position += searchIndex + 2;
                        }
                        else
                        {
                            if (await ReadToBufferAsync(token).ConfigureAwait(false) == 0)
                            {
                                return new ReadResult<T>(false, default);
                            }

                            if (currentCharBufferMemory.Span[0] == '\n')
                            {
                                _position += 1;
                            }
                        }
                    }
                    else
                    {
                        _position += searchIndex + 1;
                    }

                    if (searchIndex > spaceRemaining)
                    {
                        return new ReadResult<T>(false, default);
                    }

                    currentCharBufferMemory[..searchIndex].CopyTo(resultCharBufferMemory.Slice(resultCharBufferLength, searchIndex));
                    resultCharBufferLength += searchIndex;
                    break;
                }

                // Not found so let append into the buffer
                if (resultCharBufferLength + currentCharBufferSpan.Length <= maxLength)
                {
                    currentCharBufferSpan.CopyTo(resultCharBufferMemory.Slice(resultCharBufferLength, currentCharBufferSpan.Length).Span);
                    resultCharBufferLength += currentCharBufferSpan.Length;
                }

                _position += currentCharBufferSpan.Length;
            }

            // Read More
            if (await ReadToBufferAsync(token).ConfigureAwait(false) == 0)
            {
                return new ReadResult<T>(false, default);
            }
        } while (true);

        return T.TryParse(resultCharBufferMemory[..resultCharBufferLength].Span, CultureInfo.InvariantCulture, out var result)
            ? new ReadResult<T>(true, result)
            : new ReadResult<T>(false, default);
    }
}