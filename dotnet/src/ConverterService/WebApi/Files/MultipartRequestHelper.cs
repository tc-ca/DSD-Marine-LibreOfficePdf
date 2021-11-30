using Microsoft.Net.Http.Headers;

namespace ConverterService.WebApi.Files
{
    /// <summary>
    /// Contains helper methods for working with multipart/form-data requests.
    /// </summary>
    public static class MultipartRequestHelper
    {
        /// <summary>
        /// Returns boundary of multipart/form-data requests.
        /// </summary>
        /// <param name="contentType">Content type header value.</param>
        /// <param name="lengthLimit">Length limit.</param>
        /// <returns>Boundary string.</returns>
        /// <remarks>
        /// Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
        /// The spec at https://tools.ietf.org/html/rfc2046#section-5.1 states that 70 characters is a reasonable limit.
        /// </remarks>
        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidDataException("Missing content-type boundary");
            }

            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException(
                    $"Multipart boundary length limit {lengthLimit} exceeded");
            }

            return boundary;
        }

        /// <summary>
        /// Returns boolean indicating whether content type is a multipart content type.
        /// </summary>
        /// <param name="contentType">Content type value.</param>
        /// <returns>True if the content type is multipart, false otherwise.</returns>
        public static bool IsMultipartContentType(string contentType)
        {
            return !string.IsNullOrEmpty(contentType)
                && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) > -1;
        }

        /// <summary>
        /// Returns boolean indicating whether content disposition is set to form data.
        /// </summary>
        /// <param name="contentDisposition">Content Disposition header value.</param>
        /// <returns>True if the content disposition is set to form data, false otherwise.</returns>
        /// <remarks>
        /// Content-Disposition: form-data; name="key";
        /// </remarks>
        public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            return contentDisposition != null
                && contentDisposition.DispositionType.Equals("form-data")
                && string.IsNullOrEmpty(contentDisposition.FileName.Value)
                && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
        }

        /// <summary>
        /// Returns boolean indicating whether content disposition has files.
        /// </summary>
        /// <param name="contentDisposition">Content Disposition header value.</param>
        /// <returns>True if content disposition has files, false otherwise.</returns>
        /// <remarks>
        /// Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
        /// </remarks>
        public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            return contentDisposition != null
                && contentDisposition.DispositionType.Equals("form-data")
                && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                    || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
        }
    }
}
