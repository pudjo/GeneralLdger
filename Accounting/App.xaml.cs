using Accounting.Domain.Enum;
using Accounting.DTO;
using Accounting.IRepositories;
using Accounting.IRepositories.AR;
using Accounting.IRepositories.Inventory;
using Accounting.Repositories.SQLLite;
using Accounting.Repositories.SQLLite.AccountngRepository;

using Accounting.Repositories.SQLLite.Administration;
using Accounting.Repositories.SQLLite.AR;
using Accounting.Repositories.SQLLite.ERP;
using Accounting.Repositories.SQLLite.Inventory;
using Accounting.Repositories.SQLLite.Master;
using Accounting.Services.Administration;
using Accounting.Services.Accounts;
using Accounting.Services.AR;
using Accounting.Services.ERP;
using Accounting.Services.GeneralLedgerService;
using Accounting.Services.Inventory;
using Accounting.Services.JurnalServices;
using Accounting.Services.Validation;
using Accounting.ViewModels;
using Accounting.ViewModels.Akun;
using Accounting.ViewModels.AR;
using Accounting.ViewModels.ERP;
using Accounting.ViewModels.GeneralLedger;
using Accounting.ViewModels.Inventory;
using Accounting.ViewModels.Jurnal;
using Accounting.ViewModels.UserVM;
using Accounting.Views.AR;
using Accounting.Views.ERP;
using Accounting.Views.Inventory;
using Accounting.Views.UserView;
using ControlzEx.Standard;
using FluentValidation;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Data;
using System.IO;
using System.Windows;


namespace Accounting
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        
        public static IServiceProvider ServiceProvider { get; private set; }
        public App()
        {
            //var services = new ServiceCollection();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            

            ServiceProvider = serviceCollection.BuildServiceProvider();

            // Membuka Windows utama menggunakan DI
            var loginWindow = ServiceProvider.GetRequiredService<LoginView>();
            loginWindow.Show();

        }

        private void ConfigureServices(IServiceCollection services)
        {
           
            
            
            string rawConnectionString = "Data Source=Database\\accounting1.db";
            // debug
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string connectionString = rawConnectionString.Replace("Data Source=", $"Data Source={baseDirectory}");
            Func<IDbConnection> databaseFactory = () => new SqliteConnection(connectionString);
            services.AddTransient<IUnitOfWork>(provider => new UnitOfWork(connectionString));
            services.AddScoped<Func<IDbConnection>>(provider => () =>
            {
                var uow = provider.GetRequiredService<IUnitOfWork>();
                return uow.GetConnection(); // Mengembalikan koneksi yang dikelola UoW
            });

            services.AddScoped<Func<IDbTransaction>>(provider => () =>
            {
                var uow = provider.GetRequiredService<IUnitOfWork>();
                return uow.GetTransaction(); // Mengembalikan transaksi yang dikelola UoW
            });

            // Daftarkan Repository dan Window
            services.AddScoped<IJournalRepository, JurnalRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IAccountQueryRepository, AccountQueryRepository>();
            services.AddScoped<IGeneralLedgerQueryRepository, GeneralLedgerQueryRepository>();
            services.AddScoped<AccountService>();
            services.AddScoped<AkunViewModel>();
            services.AddScoped<AkunSelectionWindowVewModel>();
            services.AddScoped<IAccountService, AccountService>();
        
            services.AddScoped<IJournalRepository, JurnalRepository>();
            services.AddScoped<IJournalReadRepository, JournalReadRepository>();
            services.AddScoped<JurnalReadService>();
            services.AddScoped<IWriteSaldoAwalRepository, WriteSaldoAwalRepository>();
            services.AddScoped<CreatePasswordView>();
            services.AddScoped<GeneralLedgerLaporanViewModel>();
            services.AddScoped<LabaRugiViewModel>();


            services.AddScoped<IAccountQueryRepository, AccountQueryRepository>();

            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<IContactQueryRepository, ContactQueryRepository>();
            // register contact repositories (ensure these implementations exist)
            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<IContactQueryRepository, ContactQueryRepository>();
            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<IContactQueryRepository, ContactQueryRepository>();

            // Register concrete contact services
            services.AddScoped<ICustomerService,CustomerService>();
            services.AddScoped<IJurnalService, JurnalService>();
            services.AddScoped<VendorService>();

            // Register IContactService factory (returns appropriate concrete service based on ContactType)
            services.AddScoped<Func<Accounting.Domain.Enum.ContactType, IContactService>>(sp =>
            {
                return (contactType) =>
                {
                    return contactType switch
                    {
                        Accounting.Domain.Enum.ContactType.Vendor => sp.GetRequiredService<VendorService>(),
                        Accounting.Domain.Enum.ContactType.Customer => sp.GetRequiredService<CustomerService>(),
                        _ => sp.GetRequiredService<CustomerService>()
                    };
                };
            });

            // Register FluentValidation validator (optional, if you use it)
            services.AddSingleton<FluentValidation.IValidator<Accounting.DTO.ContactDTO>, Accounting.Services.Validation.ContactDtoValidator>();

            // Register factory that creates ContactListViewModel for a given ContactType
            services.AddScoped<Func<ContactType, ContactListViewModel>>(sp =>
            {
                return (contactType) =>
                {
                    var queryRepo = sp.GetRequiredService<IContactQueryRepository>();
                    var serviceFactory = sp.GetRequiredService<Func<Accounting.Domain.Enum.ContactType, IContactService>>();
                    var logger = sp.GetRequiredService<ILogger<Accounting.ViewModels.ERP.ContactListViewModel>>();
                    var validator = sp.GetService<FluentValidation.IValidator<Accounting.DTO.ContactDTO>>();
                    return new ContactListViewModel(queryRepo, serviceFactory, logger, validator, contactType);
                };
            });

            // Register views so MainWindow can resolve them if needed
            services.AddScoped<ContactListView>();
            services.AddScoped<ContactDetailWindow>();            //services.AddScoped<IWriteSaldoAwalRepository>(sp => new WriteSaldoAwalRepository(databaseFactory));

            //services.AddScoped<CustomerEntryView>();
            //services.AddScoped<CustomerEntryViewModel>();

            // Provide factory for the view so VM can create it
            //services.AddScoped<Func<CustomerEntryView>>(sp => () => sp.GetRequiredService<CustomerEntryView>());

            services.AddTransient<CustomerEntryViewModel>();
            services.AddTransient<CustomerEntryView>();

            // Factory that resolves a fresh instance each invocation
            services.AddScoped<Func<CustomerEntryView>>(sp => () => sp.GetRequiredService<Accounting.Views.AR.CustomerEntryView>());




            services.AddScoped<AkunSelectionWindowVewModel>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<UserListViewModel>();
            services.AddScoped<LoginViewModel>();
            services.AddScoped<JurnalViewModel>();
            services.AddScoped<LoginView>();
            services.AddScoped<UserService>();
            services.AddScoped<AccountService>();
            services.AddScoped<GeneralLedgerService>();
            services.AddScoped<GeneralLedgerViewModel>();
            services.AddScoped<JurnalService>();
            services.AddScoped<JurnalEntryViewModel>();
            services.AddScoped<IWriteSaldoAwalRepository, WriteSaldoAwalRepository>();
            services.AddScoped<ISaldoAwalReadRepository, SaldoAwalReadRepository>();
            services.AddScoped<SaldoAwalService>();
            services.AddScoped<InputSaldoAwalViewModel>();
            services.AddScoped<SaldoAwalReadService>();
            services.AddScoped<ICashFlowQueryRepository, CashFlowQueryRepository>();
            services.AddScoped<ICashFlowItemRepository, CashFlowItemRepository>();
            services.AddScoped<CashFlowService>();
            services.AddScoped<CashFlowItemViewModel>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductQueryRepository, ProductQueryRepository>();
            services.AddScoped<IProductService, ProductService>();
            
            services.AddScoped<ProductDetailViewModel>();
            
            services.AddScoped<ProductDetailWindow>();
            services.AddTransient<ProductListView>(); 
            services.AddTransient<ProductListViewModel>();
            services.AddTransient<ProductListDialogWindow>();

            // Kategory

            services.AddScoped<IKategoriQueryRepository, KategoriQueryRepository>();
            services.AddScoped<IKategoriRepository, KategoriRepository>();
            services.AddScoped<IKategoriService, KategoriService>();
            services.AddScoped<KategoriListViewModel>();
            services.AddScoped<KategoriDetailViewModel>();
            services.AddScoped<KategoriListView>();
            services.AddScoped<KategoriDetailWindow>();

            services.AddScoped<ISubKategoriQueryRepository, SubKategoriQueryRepository>();
            services.AddScoped<ISubKategoriRepository, SubKategoriRepository>();
            services.AddScoped<ISubKategoriService, SubKategoriService>();
            services.AddScoped<SubKategoriListViewModel>();
            services.AddScoped<SubKategoriDetailViewModel>();
            services.AddScoped<SubKategoriListView>();
            services.AddScoped<SubKategoriDetailWindow>();

            services.AddScoped<IJenisRepository, JenisRepository>(); services.AddScoped<IJenisQueryRepository, JenisQueryRepository>(); services.AddScoped<IJenisService, JenisService>(); services.AddScoped<Accounting.Views.Inventory.JenisTreeView>(); services.AddScoped<Accounting.Views.Inventory.JenisDetailWindow>(); services.AddScoped<JenisTreeViewModel>(); services.AddScoped<JenisDetailViewModel>();
            services.AddScoped<IPriceLogRepository, PriceLogRepository>();
            services.AddScoped<IPriceLogQueryRepository, PriceLogQueryRepository>();
            services.AddScoped<IPriceLogService, PriceLogService>();
            services.AddScoped<ProductPriceListView>();
            services.AddScoped<ProductPriceViewModel>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<CustomerListViewModel>();

            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ListPenjualanViewModel>();
            services.AddTransient<PenjualanDetailViewModel>();
        	services.AddTransient<PenjualanDetailView>();

            // viewmodels and views
            services.AddScoped<Accounting.ViewModels.AR.CustomerEntryViewModel>();
            services.AddScoped<Accounting.Views.AR.CustomerEntryView>();

            // factory (optional) so viewmodel can create the view
            //services.AddScoped<Func<Accounting.Views.AR.CustomerEntryView>>(sp => () => sp.GetRequiredService<Accounting.Views.AR.CustomerEntryView>());


            services.AddScoped<IPenjualanRepository, PenjualanRepository>();
            services.AddScoped<IPenjualanQueryRepository, PenjualanQueryRepository>();
            services.AddScoped<IPenjualanService, PenjualanService>();
            services.AddScoped<ListPenjualanView>();

            services.AddTransient<PenjualanDetailView>();
            services.AddTransient<PenjualanDetailViewModel>();

            // Register the factory delegate (if your DI container doesn't automatically map Func<T>)
            services.AddTransient<Func<PenjualanDetailView>>(provider => () => provider.GetRequiredService<PenjualanDetailView>());

            services.AddScoped<IJenisRepository, JenisRepository>(); services.AddScoped<IJenisQueryRepository, JenisQueryRepository>(); services.AddScoped<IJenisService, JenisService>(); services.AddScoped<Accounting.Views.Inventory.JenisTreeView>(); services.AddScoped<Accounting.Views.Inventory.JenisDetailWindow>(); services.AddScoped<JenisTreeViewModel>(); services.AddScoped<JenisDetailViewModel>();
            // Configuring Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug() // Mencatat semua dari level Debug ke atas
                .WriteTo.File("logs/log_aset_.txt", rollingInterval: RollingInterval.Day) // File baru setiap hari
                .CreateLogger();
            
            
            // 2. Add to ServiceCollection
            services.AddLogging(builder =>
            {
                builder.AddSerilog(dispose: true);
            });

            services.AddScoped<MainWindow>();


        }

    }

}
