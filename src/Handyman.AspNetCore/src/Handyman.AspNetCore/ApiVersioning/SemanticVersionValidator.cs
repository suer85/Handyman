﻿using Microsoft.Extensions.Primitives;

namespace Handyman.AspNetCore.ApiVersioning
{
    internal class SemanticVersionValidator : IApiVersionValidator
    {
        public bool Validate(string version, bool optional, StringValues validVersions, out string matchedVersion,
            out string customProblemDetail)
        {
            matchedVersion = null;
            customProblemDetail = null;

            var parserResult = SemanticVersionParser.Parse(validVersions);

            if (version == string.Empty)
            {
                if (optional)
                    return true;

                customProblemDetail = parserResult.ValidationError;
                return false;
            }

            if (!SemanticVersionParser.TryParse(version, out var semanticVersion))
            {
                customProblemDetail = parserResult.ValidationError;
                return false;
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < parserResult.DeclaredVersions.Length; i++)
            {
                var declaredVersion = parserResult.DeclaredVersions[i];

                if (semanticVersion.Major != declaredVersion.SemanticVersion.Major)
                    continue;

                var comparison = SemanticVersionComparer.Default.Compare(semanticVersion, declaredVersion.SemanticVersion);

                if (comparison > 0)
                    continue;

                matchedVersion = declaredVersion.String;
                return true;
            }

            customProblemDetail = parserResult.ValidationError;
            return false;
        }
    }
}