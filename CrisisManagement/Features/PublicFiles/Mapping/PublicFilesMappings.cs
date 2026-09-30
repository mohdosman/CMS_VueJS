using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Features.PublicFiles.ViewModels;

namespace CrisisManagement.Features.PublicFiles.Mapping;

public static class PublicFilesMappings
{
    public static HelpFileContentViewModel ToContentViewModel(this Document source) =>
        new(source.FileName, source.MIMEType, source.FileContent);
}
