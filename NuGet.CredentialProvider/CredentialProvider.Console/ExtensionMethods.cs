﻿using System;
using System.Diagnostics;
using System.Text;

namespace CredentialProvider
{
    /// <summary>
    /// Represents the set of extension methods used by this project.
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Writes a <see cref="TraceEventType.Error"/> event message to the trace listeners in the <see cref="TraceSource.Listeners" /> collection using the specified message.
        /// </summary>
        /// <param name="traceSource">A <see cref="TraceSource"/> instance to write the event to.</param>
        /// <param name="message">The error message.</param>
        public static void Error(this TraceSource traceSource, string message)
        {
            traceSource.TraceEvent(TraceEventType.Error, 0, message);
        }

        /// <summary>
        /// Writes an <see cref="TraceEventType.Information"/> event message to the trace listeners in the <see cref="TraceSource.Listeners" /> collection using the specified message.
        /// </summary>
        /// <param name="traceSource">A <see cref="TraceSource"/> instance to write the event to.</param>
        /// <param name="message">The message to write.</param>
        public static void Info(this TraceSource traceSource, string message)
        {
            traceSource.TraceEvent(TraceEventType.Information, 0, message);
        }

        /// <summary>
        /// Converts the current string to a JSON web access token (JWT) as a string.
        /// </summary>
        /// <param name="accessToken">The current access token as a string.</param>
        /// <returns>A JWT as a JSON string.</returns>
        public static string ToJsonWebTokenString(this string accessToken)
        {
            // Effictively this splits by '.' and converts from a base-64 encoded string.  Splitting creates new strings so this just calculates
            // a substring instead to reduce memory overhead.

            int start = accessToken.IndexOf(".", StringComparison.Ordinal) + 1;

            if (start < 0)
            {
                return null;
            }

            int length = accessToken.IndexOf(".", start, StringComparison.Ordinal) - start;

            return start > 0 && length < accessToken.Length
                ? Encoding.UTF8.GetString(
                    Convert.FromBase64String(
                        accessToken.Substring(start, length)
                            .PadRight(length % 2 == 1 ? length + 1 : length, '=')))
                : null;
        }

        /// <summary>
        /// Writes a <see cref="TraceEventType.Verbose"/> event message to the trace listeners in the <see cref="TraceSource.Listeners" /> collection using the specified message.
        /// </summary>
        /// <param name="traceSource">A <see cref="TraceSource"/> instance to write the event to.</param>
        /// <param name="message">The message to write.</param>
        public static void Verbose(this TraceSource traceSource, string message)
        {
            traceSource.TraceEvent(TraceEventType.Verbose, 0, message);
        }
    }
}