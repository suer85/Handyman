﻿using Microsoft.Extensions.Primitives;

namespace Handyman.AspNetCore.ApiVersioning
{
    public class ApiVersionValidator : IApiVersionValidator
    {
        public bool Validate(string version, bool optional, StringValues validVersions, out string matchedVersion, out string error)
        {
            matchedVersion = null;
            error = null;

            if (optional && string.IsNullOrEmpty(version))
                return true;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < validVersions.Count; i++)
            {
                var validVersion = validVersions[i];
                if (version != validVersion) continue;
                matchedVersion = validVersion;
                return true;
            }

            error = $"Invalid api version, supported versions: {validVersions}.";
            return false;
        }
    }
}