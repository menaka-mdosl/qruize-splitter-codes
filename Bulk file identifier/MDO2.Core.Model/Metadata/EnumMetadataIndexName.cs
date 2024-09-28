using System;

namespace MDO2.Core.Model.Metadata
{
    public enum MetadataIndexName
    {
        [MetadataIndexName("Hotels")]
        Hotels,
        [MetadataIndexName("Report Name")]
        ReportName,
        [MetadataIndexName("Date", Format = "MM-dd-yyyy", Type = typeof(DateTime))]
        BusinessDate,
        [MetadataIndexName("Report Category")]
        ReportCategory,
        [MetadataIndexName("Document Signed")]
        DocumentSigned,
        [MetadataIndexName("Notify")]
        Notify,
        [MetadataIndexName("Notes")]
        Notes,
        [MetadataIndexName("Document Signed By")]
        DocumentSignedBy,
        [MetadataIndexName("Signed Date And Time")]
        SignedDateAndTime,
        [MetadataIndexName("Connector ID")]
        ConnectorID,
        [MetadataIndexName("Management Group")]
        ManagementGroup,
        [MetadataIndexName("Source File Name")]
        SourceFileName,
        [MetadataIndexName("File Name")]
        FileName,
        [MetadataIndexName("File Size In Bytes", Type = typeof(long))]
        FileSizeInBytes,
        [MetadataIndexName("File Uploaded Date", Format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'")]
        FileUploadedDate,
        [MetadataIndexName("File Extension")]
        FileExtension,
        [MetadataIndexName("PMS")]
        PMS,
        [MetadataIndexName("Portfolio")]
        Portfolio,
        [MetadataIndexName("Uploaded By")]
        UploadedBy,
        [MetadataIndexName("User Bulk Selection")]
        UserBulkSelection,
        [Obsolete("Use 'ReportCategory'")]
        [MetadataIndexName("Category")]
        Category,
        [MetadataIndexName("Submission Schedule")]
        SubmissionSchedule,
    }
}
