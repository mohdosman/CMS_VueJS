using CrisisManagement.Data.Models;
using CrisisManagement.Data.Models.Auth;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Context;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, int,
        IdentityUserClaim<int>, ApplicationUserRole, IdentityUserLogin<int>,
        IdentityRoleClaim<int>, IdentityUserToken<int>>(options)
{
    // Who is stamped into the audit columns (SafetyNet pattern). An explicit CurrentUserId wins; otherwise
    // HttpUnitOfWork supplies a resolver that reads the signed-in user at save time. Resolving at save time
    // matters: Identity can construct the unit of work during authentication, before the user is known.
    public int CurrentUserId { get; set; }
    public Func<int>? CurrentUserIdResolver { get; set; }

    // Entity set list ported from the Blazor CMS AppDbContext. The database is pre-existing,
    // so there are no EF migrations here.
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<PermissionGroup> PermissionGroupNames => Set<PermissionGroup>();
    public DbSet<Logon> Logons => Set<Logon>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<PasswordChangeLog> PasswordChangeLogs => Set<PasswordChangeLog>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ReportLog> ReportLogs => Set<ReportLog>();
    public DbSet<TempTrigger> TempTriggers => Set<TempTrigger>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<AddressType> AddressTypes => Set<AddressType>();
    public DbSet<AppNotification> AppNotifications => Set<AppNotification>();
    public DbSet<AppSupport> AppSupports => Set<AppSupport>();
    public DbSet<AssessmentLocation> AssessmentLocations => Set<AssessmentLocation>();
    public DbSet<AssessmentType> AssessmentTypes => Set<AssessmentType>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<County> Counties => Set<County>();
    public DbSet<CriminalJusticeStatus> CriminalJusticeStatuses => Set<CriminalJusticeStatus>();
    public DbSet<CurrentService> CurrentServices => Set<CurrentService>();
    public DbSet<DSM4Code> DSM4Codes => Set<DSM4Code>();
    public DbSet<DSM4CodeType> DSM4CodeTypes => Set<DSM4CodeType>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Disposition> Dispositions => Set<Disposition>();
    public DbSet<DivisionMaxLiability> DivisionMaxLiabilities => Set<DivisionMaxLiability>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<Drug> Drugs => Set<Drug>();
    public DbSet<DrugFrequency> DrugFrequencies => Set<DrugFrequency>();
    public DbSet<DrugRoute> DrugRoutes => Set<DrugRoute>();
    public DbSet<EducationLevel> EducationLevels => Set<EducationLevel>();
    public DbSet<EmploymentStatus> EmploymentStatuses => Set<EmploymentStatus>();
    public DbSet<EndUserAgreement> EndUserAgreements => Set<EndUserAgreement>();
    public DbSet<Ethnicity> Ethnicities => Set<Ethnicity>();
    public DbSet<EvaluationCategory> EvaluationCategories => Set<EvaluationCategory>();
    public DbSet<EvaluationType> EvaluationTypes => Set<EvaluationType>();
    public DbSet<F2FAssessment> F2FAssessments => Set<F2FAssessment>();
    public DbSet<F2FDrug> F2FDrugs => Set<F2FDrug>();
    public DbSet<F2FEvaluation> F2FEvaluations => Set<F2FEvaluation>();
    public DbSet<F2FHospAlternative> F2FHospAlternatives => Set<F2FHospAlternative>();
    public DbSet<F2FHospitalization> F2FHospitalizations => Set<F2FHospitalization>();
    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<FacilityUser> FacilityUsers => Set<FacilityUser>();
    public DbSet<FileUpload> FileUploads => Set<FileUpload>();
    public DbSet<FileUploadError> FileUploadErrors => Set<FileUploadError>();
    public DbSet<FileUploadErrorCode> FileUploadErrorCodes => Set<FileUploadErrorCode>();
    public DbSet<FileUploadErrorTable> FileUploadErrorTables => Set<FileUploadErrorTable>();
    public DbSet<FileUploadF2FAssessment> FileUploadF2FAssessments => Set<FileUploadF2FAssessment>();
    public DbSet<FileUploadF2FDrug> FileUploadF2FDrugs => Set<FileUploadF2FDrug>();
    public DbSet<FileUploadF2FHospAlternative> FileUploadF2FHospAlternatives => Set<FileUploadF2FHospAlternative>();
    public DbSet<FileUploadF2FHospitalization> FileUploadF2FHospitalizations => Set<FileUploadF2FHospitalization>();
    public DbSet<FileUploadPatient> FileUploadPatients => Set<FileUploadPatient>();
    public DbSet<FileUploadPhoneAssessment> FileUploadPhoneAssessments => Set<FileUploadPhoneAssessment>();
    public DbSet<FiscalYear> FiscalYears => Set<FiscalYear>();
    public DbSet<Gender> Genders => Set<Gender>();
    public DbSet<HospAltDisposition> HospAltDispositions => Set<HospAltDisposition>();
    public DbSet<HospAltDispositionList> HospAltDispositionLists => Set<HospAltDispositionList>();
    public DbSet<Hospitalization> Hospitalizations => Set<Hospitalization>();
    public DbSet<HospitalizationAlternative> HospitalizationAlternatives => Set<HospitalizationAlternative>();
    public DbSet<HospitalizationDisposition> HospitalizationDispositions => Set<HospitalizationDisposition>();
    public DbSet<MaritalStatus> MaritalStatuses => Set<MaritalStatus>();
    public DbSet<MilitaryStatus> MilitaryStatuses => Set<MilitaryStatus>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientAddress> PatientAddresses => Set<PatientAddress>();
    public DbSet<PayorSource> PayorSources => Set<PayorSource>();
    public DbSet<PhoneAssessment> PhoneAssessments => Set<PhoneAssessment>();
    public DbSet<PrimaryProblem> PrimaryProblems => Set<PrimaryProblem>();
    public DbSet<CrisisManagement.Data.Models.Domain.Program> Programs => Set<CrisisManagement.Data.Models.Domain.Program>();
    public DbSet<ProgramMaxLiability> ProgramMaxLiabilities => Set<ProgramMaxLiability>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<ProviderAddress> ProviderAddresses => Set<ProviderAddress>();
    public DbSet<ProviderUser> ProviderUsers => Set<ProviderUser>();
    public DbSet<Race> Races => Set<Race>();
    public DbSet<RecommendedTransportMode> RecommendedTransportModes => Set<RecommendedTransportMode>();
    public DbSet<ResidentialStatus> ResidentialStatuses => Set<ResidentialStatus>();
    public DbSet<SchoolAttendence> SchoolAttendences => Set<SchoolAttendence>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceCode> ServiceCodes => Set<ServiceCode>();
    public DbSet<ServiceFile> ServiceFiles => Set<ServiceFile>();
    public DbSet<ServiceFileError> ServiceFileErrors => Set<ServiceFileError>();
    public DbSet<ServiceFileErrorCode> ServiceFileErrorCodes => Set<ServiceFileErrorCode>();
    public DbSet<ServiceFileImport> ServiceFileImports => Set<ServiceFileImport>();
    public DbSet<State> States => Set<State>();
    public DbSet<SuicideFile> SuicideFiles => Set<SuicideFile>();
    public DbSet<SuicideFileImport> SuicideFileImports => Set<SuicideFileImport>();
    public DbSet<YesNoUnknown> YesNoUnknowns => Set<YesNoUnknown>();
    public DbSet<AppSessionEntity> AppSessions => Set<AppSessionEntity>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        Audit();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct = default)
    {
        Audit();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, ct);
    }

    // CurrentUserId is set by HttpUnitOfWork. The CreatedBy/UpdatedBy columns are NOT NULL, so
    // unauthenticated writes (sign-in bookkeeping) stamp 0.
    private void Audit()
    {
        var now = DateTime.Now;
        var uid = CurrentUserId != 0 ? CurrentUserId : CurrentUserIdResolver?.Invoke() ?? 0;

        foreach (var e in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (e.State == EntityState.Added)
            {
                e.Entity.CreatedOn = e.Entity.UpdatedOn = now;
                e.Entity.CreatedBy = e.Entity.UpdatedBy = uid;
            }
            else if (e.State == EntityState.Modified)
            {
                e.Property(x => x.CreatedOn).IsModified = false;
                e.Property(x => x.CreatedBy).IsModified = false;
                e.Entity.UpdatedOn = now;
                e.Entity.UpdatedBy = uid;
            }
        }
    }
}
