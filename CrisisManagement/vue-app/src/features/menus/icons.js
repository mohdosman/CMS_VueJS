// RBS_MenuItem.Icon holds MudBlazor icon ids ("Icons.Material.Filled.Home"); the SPA uses Font Awesome 4.
// Same table as the server-side MenuService, so an icon chosen here is the one the navbar shows.
const FA = {
    Home: 'fa-home', Description: 'fa-file-text-o', MedicalServices: 'fa-medkit', Emergency: 'fa-ambulance',
    Assessment: 'fa-bar-chart', AdminPanelSettings: 'fa-cogs', ChangePassword: 'fa-lock', Security: 'fa-shield',
    SettingsApplications: 'fa-cog'
};

export const iconOptions = Object.keys(FA).map((k) => `Icons.Material.Filled.${k}`);
export const iconClass = (icon) => (icon ? FA[icon.split('.').pop()] ?? '' : '');
