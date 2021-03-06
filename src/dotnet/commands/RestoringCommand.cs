// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.DotNet.Tools.MSBuild;
using Microsoft.DotNet.Tools.Restore;

namespace Microsoft.DotNet.Tools
{
    public class RestoringCommand : MSBuildForwardingApp
    {
        private bool NoRestore { get; }

        private IEnumerable<string> ArgsToForward { get; }

        private IEnumerable<string> ArgsToForwardToRestore()
        {
            var restoreArguments = ArgsToForward.Where(a =>
                !a.StartsWith("/t:") &&
                !a.StartsWith("/target:") &&
                !a.StartsWith("/ConsoleLoggerParameters:") &&
                !a.StartsWith("/clp:"));

            if (!restoreArguments.Any(a => a.StartsWith("/v:") || a.StartsWith("/verbosity:")))
            {
                restoreArguments = restoreArguments.Concat(new string[] { "/v:q" });
            }

            return restoreArguments;
        }

        private bool ShouldRunImplicitRestore => !NoRestore;

        public RestoringCommand(IEnumerable<string> msbuildArgs, bool noRestore, string msbuildPath = null)
            : base(msbuildArgs, msbuildPath)
        {
            NoRestore = noRestore;
            ArgsToForward = msbuildArgs;
        }

        public override int Execute()
        {
            if (ShouldRunImplicitRestore)
            {
                int exitCode = RestoreCommand.Run(ArgsToForwardToRestore().ToArray());
                if (exitCode != 0)
                {
                    return exitCode;
                }
            }

            return base.Execute();
        }
    }
}