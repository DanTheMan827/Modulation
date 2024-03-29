﻿using System.IO;

namespace DanTheMan827.Modulation.Helpers
{
    internal partial class HelperMethods
    {
        public long CalculateTotalFileSize(params string[] files)
        {
            long totalFileSize = 0;

            foreach (string? file in files)
            {
                var fi = new FileInfo(file);

                totalFileSize += fi.Length;
            }

            return totalFileSize;
        }
    }
}
