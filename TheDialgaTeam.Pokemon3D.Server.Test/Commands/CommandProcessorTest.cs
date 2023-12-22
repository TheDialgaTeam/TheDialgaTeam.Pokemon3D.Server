// Pokemon 3D Server Client
// Copyright (C) 2023 Yong Jian Ming
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

using JetBrains.Annotations;
using TheDialgaTeam.Pokemon3D.Server.Core.Commands;

namespace TheDialgaTeam.Pokemon3D.Server.Test.Commands;

[TestSubject(typeof(CommandProcessor))]
public class CommandProcessorTest
{
    public static readonly TheoryData<string, string[]> CommandArgumentTestResults = new()
    {
        { "Test", ["Test"] },
        { "Test Test2", ["Test", "Test2"] },
        { "Test Test2 Test3", ["Test", "Test2", "Test3"] },

        { "\"Test\"", ["Test"] },
        { "Test \"Test2\"", ["Test", "Test2"] },
        { "Test Test2 \"Test3\"", ["Test", "Test2", "Test3"] },

        { "\"Test With Space\"", ["Test With Space"] },
        { "\"Test With Space\" And", ["Test With Space", "And"] },
        { "\"Test With Space or \\\"quote\\\"\" And", ["Test With Space or \"quote\"", "And"] }
    };

    [Theory]
    [MemberData(nameof(CommandArgumentTestResults))]
    public void GetCommandArgsTest(string message, string[] result)
    {
        Assert.Equal(result, CommandProcessor.GetCommandArgs(message));
    }
}