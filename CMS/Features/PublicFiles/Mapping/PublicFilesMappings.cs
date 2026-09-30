using CMS.Data.Models.Domain;
using CMS.Features.PublicFiles.ViewModels;

namespace CMS.Features.PublicFiles.Mapping;

public static class PublicFilesMappings
{
    public static HelpFileContentViewModel ToContentViewModel(this Document source) =>
        new(source.FileName, source.MIMEType, source.FileContent);
}
