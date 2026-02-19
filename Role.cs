using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace GestionComerce
{
    public class Role
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }

        // ✅ Client Permissions
        public bool CreateClient { get; set; }
        public bool ModifyClient { get; set; }
        public bool DeleteClient { get; set; }
        public bool ViewOperationClient { get; set; }
        public bool PayeClient { get; set; }
        public bool ViewClient { get; set; }

        // ✅ Fournisseur Permissions
        public bool CreateFournisseur { get; set; }
        public bool ModifyFournisseur { get; set; }
        public bool DeleteFournisseur { get; set; }
        public bool ViewOperationFournisseur { get; set; }
        public bool PayeFournisseur { get; set; }
        public bool ViewFournisseur { get; set; }

        // ✅ Operation Permissions
        public bool ReverseOperation { get; set; }
        public bool ReverseMouvment { get; set; }
        public bool ViewOperation { get; set; }
        public bool ViewMouvment { get; set; }

        // ✅ General Views
        public bool ViewProjectManagment { get; set; }
        public bool ViewSettings { get; set; }

        // ✅ User Management
        public bool ViewUsers { get; set; }
        public bool EditUsers { get; set; }
        public bool DeleteUsers { get; set; }
        public bool AddUsers { get; set; }

        // ✅ Roles
        public bool ViewRoles { get; set; }
        public bool AddRoles { get; set; }
        public bool DeleteRoles { get; set; }

        // ✅ Familly
        public bool ViewFamilly { get; set; }
        public bool EditFamilly { get; set; }
        public bool DeleteFamilly { get; set; }
        public bool AddFamilly { get; set; }

        // ✅ Article & Ticket & Facture & Settings
        public bool AddArticle { get; set; }
        public bool DeleteArticle { get; set; }
        public bool EditArticle { get; set; }
        public bool ViewArticle { get; set; }
        public bool Repport { get; set; }
        public bool Ticket { get; set; }
        public bool SolderFournisseur { get; set; }
        public bool SolderClient { get; set; }

        public bool ViewFactureSettings { get; set; }
        public bool ModifyFactureSettings { get; set; }
        public bool ViewFacture { get; set; }

        public bool ViewPaymentMethod { get; set; }
        public bool AddPaymentMethod { get; set; }
        public bool ModifyPaymentMethod { get; set; }
        public bool DeletePaymentMethod { get; set; }

        public bool ViewApropos { get; set; }
        public bool Logout { get; set; }
        public bool ViewExit { get; set; }
        public bool ViewShutDown { get; set; }

        // ✅ NEW PERMISSIONS ADDED
        public bool ViewClientsPage { get; set; }
        public bool ViewFournisseurPage { get; set; }
        public bool ViewInventrory { get; set; }
        public bool ViewVente { get; set; }
        public bool CashClient { get; set; }
        public bool CashFournisseur { get; set; }
        public bool ViewCreditClient { get; set; }
        public bool ViewCreditFournisseur { get; set; }
        public bool ViewLivraison { get; set; }

        // ✅ Facturation Permissions
        public bool AccessFacturation { get; set; }
        public bool CreateFacture { get; set; }
        public bool HistoriqueFacture { get; set; }
        public bool HistoryCheck { get; set; }
        public bool FactureEnregistrees { get; set; }

        // ✅ Livraison Permissions
        public bool AccessLivraison { get; set; }
        public bool CreationLivraison { get; set; }
        public bool GestionLivreur { get; set; }

        private static readonly string ConnectionString =
            "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";

        // ✅ Get All Roles
        public async Task<List<Role>> GetRolesAsync()
        {
            var roles = new List<Role>();
            string query = "SELECT * FROM Role WHERE Etat=1";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var role = new Role
                        {
                            RoleID = Convert.ToInt32(reader["RoleID"]),
                            RoleName = reader["RoleName"].ToString(),

                            CreateClient = Convert.ToBoolean(reader["CreateClient"]),
                            ModifyClient = Convert.ToBoolean(reader["ModifyClient"]),
                            DeleteClient = Convert.ToBoolean(reader["DeleteClient"]),
                            ViewOperationClient = Convert.ToBoolean(reader["ViewOperationClient"]),
                            PayeClient = Convert.ToBoolean(reader["PayeClient"]),
                            ViewClient = Convert.ToBoolean(reader["ViewClient"]),

                            CreateFournisseur = Convert.ToBoolean(reader["CreateFournisseur"]),
                            ModifyFournisseur = Convert.ToBoolean(reader["ModifyFournisseur"]),
                            DeleteFournisseur = Convert.ToBoolean(reader["DeleteFournisseur"]),
                            ViewOperationFournisseur = Convert.ToBoolean(reader["ViewOperationFournisseur"]),
                            PayeFournisseur = Convert.ToBoolean(reader["PayeFournisseur"]),
                            ViewFournisseur = Convert.ToBoolean(reader["ViewFournisseur"]),

                            ReverseOperation = Convert.ToBoolean(reader["ReverseOperation"]),
                            ReverseMouvment = Convert.ToBoolean(reader["ReverseMouvment"]),
                            ViewOperation = Convert.ToBoolean(reader["ViewOperation"]),
                            ViewMouvment = Convert.ToBoolean(reader["ViewMouvment"]),

                            ViewProjectManagment = Convert.ToBoolean(reader["ViewProjectManagment"]),
                            ViewSettings = Convert.ToBoolean(reader["ViewSettings"]),

                            ViewUsers = Convert.ToBoolean(reader["ViewUsers"]),
                            EditUsers = Convert.ToBoolean(reader["EditUsers"]),
                            DeleteUsers = Convert.ToBoolean(reader["DeleteUsers"]),
                            AddUsers = Convert.ToBoolean(reader["AddUsers"]),

                            ViewRoles = Convert.ToBoolean(reader["ViewRoles"]),
                            AddRoles = Convert.ToBoolean(reader["AddRoles"]),
                            DeleteRoles = Convert.ToBoolean(reader["DeleteRoles"]),

                            ViewFamilly = Convert.ToBoolean(reader["ViewFamilly"]),
                            EditFamilly = Convert.ToBoolean(reader["EditFamilly"]),
                            DeleteFamilly = Convert.ToBoolean(reader["DeleteFamilly"]),
                            AddFamilly = Convert.ToBoolean(reader["AddFamilly"]),

                            AddArticle = Convert.ToBoolean(reader["AddArticle"]),
                            DeleteArticle = Convert.ToBoolean(reader["DeleteArticle"]),
                            EditArticle = Convert.ToBoolean(reader["EditArticle"]),
                            ViewArticle = Convert.ToBoolean(reader["ViewArticle"]),
                            Repport = Convert.ToBoolean(reader["Repport"]),
                            Ticket = Convert.ToBoolean(reader["Ticket"]),
                            SolderFournisseur = Convert.ToBoolean(reader["SolderFournisseur"]),
                            SolderClient = Convert.ToBoolean(reader["SolderClient"]),

                            ViewFactureSettings = Convert.ToBoolean(reader["ViewFactureSettings"]),
                            ModifyFactureSettings = Convert.ToBoolean(reader["ModifyFactureSettings"]),
                            ViewFacture = Convert.ToBoolean(reader["ViewFacture"]),

                            ViewPaymentMethod = Convert.ToBoolean(reader["ViewPaymentMethod"]),
                            AddPaymentMethod = Convert.ToBoolean(reader["AddPaymentMethod"]),
                            ModifyPaymentMethod = Convert.ToBoolean(reader["ModifyPaymentMethod"]),
                            DeletePaymentMethod = Convert.ToBoolean(reader["DeletePaymentMethod"]),

                            ViewApropos = Convert.ToBoolean(reader["ViewApropos"]),
                            Logout = Convert.ToBoolean(reader["Logout"]),
                            ViewExit = Convert.ToBoolean(reader["ViewExit"]),
                            ViewShutDown = Convert.ToBoolean(reader["ViewShutDown"]),

                            // ✅ EXISTING NEW FIELDS
                            ViewClientsPage = Convert.ToBoolean(reader["ViewClientsPage"]),
                            ViewFournisseurPage = Convert.ToBoolean(reader["ViewFournisseurPage"]),
                            ViewInventrory = Convert.ToBoolean(reader["ViewInventrory"]),
                            ViewVente = Convert.ToBoolean(reader["ViewVente"]),
                            CashClient = Convert.ToBoolean(reader["CashClient"]),
                            CashFournisseur = Convert.ToBoolean(reader["CashFournisseur"]),
                            ViewCreditClient = Convert.ToBoolean(reader["ViewCreditClient"]),
                            ViewCreditFournisseur = Convert.ToBoolean(reader["ViewCreditFournisseur"]),

                            // ✅ NEW FACTURATION FIELDS
                            AccessFacturation = Convert.ToBoolean(reader["AccessFacturation"]),
                            CreateFacture = Convert.ToBoolean(reader["CreateFacture"]),
                            HistoriqueFacture = Convert.ToBoolean(reader["HistoriqueFacture"]),
                            HistoryCheck = Convert.ToBoolean(reader["HistoryCheck"]),
                            FactureEnregistrees = Convert.ToBoolean(reader["FactureEnregistrees"]),

                            // ✅ NEW LIVRAISON FIELDS
                            AccessLivraison = Convert.ToBoolean(reader["AccessLivraison"]),
                            CreationLivraison = Convert.ToBoolean(reader["CreationLivraison"]),
                            GestionLivreur = Convert.ToBoolean(reader["GestionLivreur"])
                        };
                        roles.Add(role);
                    }
                }
            }
            return roles;
        }

        // ✅ Insert Role
        public async Task<int> InsertRoleAsync()
        {
            string query = @"
                INSERT INTO Role (
                    RoleName, CreateClient, ModifyClient, DeleteClient, ViewOperationClient, PayeClient, ViewClient,
                    CreateFournisseur, ModifyFournisseur, DeleteFournisseur, ViewOperationFournisseur, PayeFournisseur, ViewFournisseur,
                    ReverseOperation, ReverseMouvment, ViewOperation, ViewMouvment,
                    ViewProjectManagment, ViewSettings,
                    ViewUsers, EditUsers, DeleteUsers, AddUsers,
                    ViewRoles, AddRoles, DeleteRoles,
                    ViewFamilly, EditFamilly, DeleteFamilly, AddFamilly,
                    AddArticle, DeleteArticle, EditArticle, ViewArticle, Repport, Ticket, SolderFournisseur, SolderClient,
                    ViewFactureSettings, ModifyFactureSettings, ViewFacture,
                    ViewPaymentMethod, AddPaymentMethod, ModifyPaymentMethod, DeletePaymentMethod,
                    ViewApropos, Logout, ViewExit, ViewShutDown,
                    ViewClientsPage, ViewFournisseurPage, ViewInventrory, ViewVente, CashClient, CashFournisseur, ViewCreditClient, ViewCreditFournisseur,
                    AccessFacturation, CreateFacture, HistoriqueFacture, HistoryCheck, FactureEnregistrees,
                    AccessLivraison, CreationLivraison, GestionLivreur
                ) VALUES (
                    @RoleName, @CreateClient, @ModifyClient, @DeleteClient, @ViewOperationClient, @PayeClient, @ViewClient,
                    @CreateFournisseur, @ModifyFournisseur, @DeleteFournisseur, @ViewOperationFournisseur, @PayeFournisseur, @ViewFournisseur,
                    @ReverseOperation, @ReverseMouvment, @ViewOperation, @ViewMouvment,
                    @ViewProjectManagment, @ViewSettings,
                    @ViewUsers, @EditUsers, @DeleteUsers, @AddUsers,
                    @ViewRoles, @AddRoles, @DeleteRoles,
                    @ViewFamilly, @EditFamilly, @DeleteFamilly, @AddFamilly,
                    @AddArticle, @DeleteArticle, @EditArticle, @ViewArticle, @Repport, @Ticket, @SolderFournisseur, @SolderClient,
                    @ViewFactureSettings, @ModifyFactureSettings, @ViewFacture,
                    @ViewPaymentMethod, @AddPaymentMethod, @ModifyPaymentMethod, @DeletePaymentMethod,
                    @ViewApropos, @Logout, @ViewExit, @ViewShutDown,
                    @ViewClientsPage, @ViewFournisseurPage, @ViewInventrory, @ViewVente, @CashClient, @CashFournisseur, @ViewCreditClient, @ViewCreditFournisseur,
                    @AccessFacturation, @CreateFacture, @HistoriqueFacture, @HistoryCheck, @FactureEnregistrees,
                    @AccessLivraison, @CreationLivraison, @GestionLivreur
                );
                SELECT SCOPE_IDENTITY();";

            return await ExecuteSaveAsync(query);
        }

        // ✅ Update Role
        public async Task<int> UpdateRoleAsync()
        {
            string query = @"
                UPDATE Role SET
                    RoleName=@RoleName,
                    CreateClient=@CreateClient, ModifyClient=@ModifyClient, DeleteClient=@DeleteClient, ViewOperationClient=@ViewOperationClient, PayeClient=@PayeClient, ViewClient=@ViewClient,
                    CreateFournisseur=@CreateFournisseur, ModifyFournisseur=@ModifyFournisseur, DeleteFournisseur=@DeleteFournisseur, ViewOperationFournisseur=@ViewOperationFournisseur, PayeFournisseur=@PayeFournisseur, ViewFournisseur=@ViewFournisseur,
                    ReverseOperation=@ReverseOperation, ReverseMouvment=@ReverseMouvment, ViewOperation=@ViewOperation, ViewMouvment=@ViewMouvment,
                    ViewProjectManagment=@ViewProjectManagment, ViewSettings=@ViewSettings,
                    ViewUsers=@ViewUsers, EditUsers=@EditUsers, DeleteUsers=@DeleteUsers, AddUsers=@AddUsers,
                    ViewRoles=@ViewRoles, AddRoles=@AddRoles, DeleteRoles=@DeleteRoles,
                    ViewFamilly=@ViewFamilly, EditFamilly=@EditFamilly, DeleteFamilly=@DeleteFamilly, AddFamilly=@AddFamilly,
                    AddArticle=@AddArticle, DeleteArticle=@DeleteArticle, EditArticle=@EditArticle, ViewArticle=@ViewArticle, Repport=@Repport, Ticket=@Ticket, SolderFournisseur=@SolderFournisseur, SolderClient=@SolderClient,
                    ViewFactureSettings=@ViewFactureSettings, ModifyFactureSettings=@ModifyFactureSettings, ViewFacture=@ViewFacture,
                    ViewPaymentMethod=@ViewPaymentMethod, AddPaymentMethod=@AddPaymentMethod, ModifyPaymentMethod=@ModifyPaymentMethod, DeletePaymentMethod=@DeletePaymentMethod,
                    ViewApropos=@ViewApropos, Logout=@Logout, ViewExit=@ViewExit, ViewShutDown=@ViewShutDown,
                    ViewClientsPage=@ViewClientsPage, ViewFournisseurPage=@ViewFournisseurPage, ViewInventrory=@ViewInventrory, ViewVente=@ViewVente, 
                    CashClient=@CashClient, CashFournisseur=@CashFournisseur, ViewCreditClient=@ViewCreditClient, ViewCreditFournisseur=@ViewCreditFournisseur,
                    AccessFacturation=@AccessFacturation, CreateFacture=@CreateFacture, HistoriqueFacture=@HistoriqueFacture, HistoryCheck=@HistoryCheck, FactureEnregistrees=@FactureEnregistrees,
                    AccessLivraison=@AccessLivraison, CreationLivraison=@CreationLivraison, GestionLivreur=@GestionLivreur
                WHERE RoleID=@RoleID";

            return await ExecuteSaveAsync(query, true);
        }

        // ✅ Common Save Function
        private async Task<int> ExecuteSaveAsync(string query, bool isUpdate = false)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@RoleName", RoleName);
                    if (isUpdate) cmd.Parameters.AddWithValue("@RoleID", RoleID);

                    foreach (var prop in GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(bool))
                            cmd.Parameters.AddWithValue("@" + prop.Name, (bool)prop.GetValue(this));
                    }

                    object result = isUpdate ? await cmd.ExecuteNonQueryAsync() : await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        // ✅ Soft Delete
        public async Task<int> DeleteRoleAsync()
        {
            string query = "UPDATE Role SET Etat=0 WHERE RoleID=@RoleID";
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@RoleID", RoleID);
                    await cmd.ExecuteNonQueryAsync();
                    return 1;
                }
            }
        }
    }
}