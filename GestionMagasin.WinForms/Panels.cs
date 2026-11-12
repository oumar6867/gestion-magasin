using GestionMagasin.Application;
using GestionMagasin.Domain;

namespace GestionMagasin.WinForms;

static class Ui
{
    public static Button Button(string text)=>new(){Text=text,AutoSize=true,Padding=new Padding(12,6,12,6),BackColor=Color.FromArgb(38,110,170),ForeColor=Color.White,FlatStyle=FlatStyle.Flat};
    public static Label Title(string text)=>new(){Text=text,AutoSize=true,Font=new Font("Segoe UI",18,FontStyle.Bold),Margin=new Padding(0,0,0,14)};
    public static DataGridView Grid()=>new(){Dock=DockStyle.Fill,AutoGenerateColumns=true,ReadOnly=true,AllowUserToAddRows=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};
    public static string StockLabel(int stock,int seuil)=>stock<=0?"RUPTURE":stock<=seuil?"STOCK FAIBLE":"DISPONIBLE";
}

public sealed class ProductsPanel : UserControl
{
    readonly ProduitService service; readonly ICategorieRepository categories; readonly IProduitRepository repository; readonly Utilisateur user; readonly DataGridView grid=Ui.Grid(); readonly TextBox search=new(); readonly CheckBox archived=new(){Text="Inclure les archivés",AutoSize=true};
    public ProductsPanel(Utilisateur user,ProduitService service,ICategorieRepository categories,IProduitRepository repository){this.user=user;this.service=service;this.categories=categories;this.repository=repository;var top=new FlowLayoutPanel{Dock=DockStyle.Top,Height=78};top.Controls.Add(Ui.Title("Produits"));search.Width=220;search.PlaceholderText="Référence ou nom";search.TextChanged+=async(_,_)=>await LoadAsync();top.Controls.Add(search);archived.CheckedChanged+=async(_,_)=>await LoadAsync();top.Controls.Add(archived);if(user.Role==Roles.Administrateur){var add=Ui.Button("Nouveau produit");add.Click+=(_,_)=>AddProduct();top.Controls.Add(add);}Controls.Add(grid);Controls.Add(top);Load+=async(_,_)=>await LoadAsync();}
    async Task LoadAsync(){var rows=await service.RechercherAsync(new ProduitFilter(search.Text,null,null,archived.Checked?null:true));grid.DataSource=rows.Select(p=>new{p.Reference,p.Nom,p.PrixVenteFcfa,Statut=p.Actif?"ACTIF":"ARCHIVÉ",p.SeuilAlerte,Créé=p.CreeLeUtc.ToLocalTime().ToString("g")}).ToList();}
    void AddProduct(){using var f=new ProductDialog(categories);if(f.ShowDialog()!=DialogResult.OK)return;try{service.CreerAsync(new Produit(Guid.NewGuid(),f.Reference,f.Nom,f.CategoryId,f.Price,f.Threshold,DateTime.UtcNow)).GetAwaiter().GetResult();LoadAsync().GetAwaiter().GetResult();}catch(Exception ex){MessageBox.Show(ex.Message,"Produit",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
}

public sealed class ProductDialog : Form
{
    public string Reference=>refBox.Text.Trim();public string Nom=>nameBox.Text.Trim();public Guid? CategoryId=>cat.SelectedValue is Guid id&&id!=Guid.Empty?id:null;public long Price=>long.TryParse(price.Text,out var n)?n:0;public int Threshold=>int.TryParse(threshold.Text,out var n)?n:0;readonly TextBox refBox=new(),nameBox=new(),price=new(),threshold=new();readonly ComboBox cat=new(){DropDownStyle=ComboBoxStyle.DropDownList};
    public ProductDialog(ICategorieRepository repo){Text="Nouveau produit";Width=450;Height=330;StartPosition=FormStartPosition.CenterParent;var p=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(20),ColumnCount=2,RowCount=6};p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,38));p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,62));Add(p,"Référence",refBox);Add(p,"Nom",nameBox);Add(p,"Prix FCFA",price);Add(p,"Seuil d'alerte",threshold);p.Controls.Add(new Label{Text="Catégorie",Anchor=AnchorStyles.Left,AutoSize=true},0,4);cat.DataSource=repo.ListerAsync().GetAwaiter().GetResult().Select(x=>new{ x.Id,x.Nom}).Prepend(new{Id=Guid.Empty,Nom="(Aucune)"}).ToList();cat.DisplayMember="Nom";cat.ValueMember="Id";p.Controls.Add(cat,1,4);var b=Ui.Button("Enregistrer");b.Click+=(_,_)=>{if(string.IsNullOrWhiteSpace(Reference)||string.IsNullOrWhiteSpace(Nom)||Price<=0){MessageBox.Show("Référence, nom et prix strictement positif sont obligatoires.");return;}DialogResult=DialogResult.OK;Close();};p.Controls.Add(b,0,5);p.SetColumnSpan(b,2);Controls.Add(p);}
    static void Add(TableLayoutPanel p,string t,TextBox b){p.Controls.Add(new Label{Text=t,Anchor=AnchorStyles.Left,AutoSize=true});b.Dock=DockStyle.Fill;p.Controls.Add(b);}
}

public sealed class CategoriesPanel : UserControl
{
    readonly ICategorieRepository repo;readonly Utilisateur user;readonly DataGridView grid=Ui.Grid();public CategoriesPanel(Utilisateur user,ICategorieRepository repo){this.user=user;this.repo=repo;var top=new FlowLayoutPanel{Dock=DockStyle.Top,Height=62};top.Controls.Add(Ui.Title("Catégories"));if(user.Role==Roles.Administrateur){var b=Ui.Button("Nouvelle catégorie");b.Click+=async(_,_)=>{using var dialog=new CategoryDialog();if(dialog.ShowDialog()!=DialogResult.OK)return;try{await repo.AjouterAsync(new Categorie(Guid.NewGuid(),dialog.NameValue));await LoadAsync();}catch(Exception ex){MessageBox.Show(ex.Message);}};top.Controls.Add(b);}Controls.Add(grid);Controls.Add(top);Load+=async(_,_)=>await LoadAsync();}
    async Task LoadAsync(){grid.DataSource=(await repo.ListerAsync()).Select(x=>new{x.Id,x.Nom,Statut=x.Active?"ACTIVE":"ARCHIVÉ"}).ToList();}
}
public sealed class CategoryDialog:Form
{
    readonly TextBox name=new();public string NameValue=>name.Text.Trim();public CategoryDialog(){Text="Nouvelle catégorie";Width=360;Height=150;StartPosition=FormStartPosition.CenterParent;var p=new FlowLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(15)};p.Controls.Add(new Label{Text="Nom",AutoSize=true});name.Width=200;p.Controls.Add(name);var b=Ui.Button("Créer");b.Click+=(_,_)=>{if(!string.IsNullOrWhiteSpace(NameValue)){DialogResult=DialogResult.OK;Close();}};p.Controls.Add(b);Controls.Add(p);}
}

public sealed class StockPanel : UserControl
{
    readonly Utilisateur user;readonly ProduitService products;readonly StockService stock;readonly DataGridView grid=Ui.Grid();public StockPanel(Utilisateur user,ProduitService products,StockService stock){this.user=user;this.products=products;this.stock=stock;var top=new FlowLayoutPanel{Dock=DockStyle.Top,Height=62};top.Controls.Add(Ui.Title("Mouvements de stock"));if(user.Role==Roles.Administrateur){var b=Ui.Button("Nouveau mouvement");b.Click+=async(_,_)=>await AddAsync();top.Controls.Add(b);}Controls.Add(grid);Controls.Add(top);Load+=async(_,_)=>await LoadAsync();}
    async Task LoadAsync(){grid.DataSource=(await stock.ListerAsync()).Select(x=>new{x.Mouvement.ProduitId,Type=x.Mouvement.Type,x.Mouvement.Quantite,x.Mouvement.Motif,x.Mouvement.DateTechniqueUtc,Stock=x.StockApres}).ToList();}
    async Task AddAsync(){var productsList=await products.RechercherAsync(new ProduitFilter(Actif:true));if(productsList.Count==0){MessageBox.Show("Créez d'abord un produit.");return;}using var f=new MovementDialog(productsList);if(f.ShowDialog()!=DialogResult.OK)return;try{var q=f.Type==TypesMouvement.Correction?f.Counted-f.Current:f.Quantity;await stock.EnregistrerAsync(user,new MouvementStock(Guid.NewGuid(),f.ProductId,q,f.Type,f.Reason,user.Id,DateTime.UtcNow,f.Current,f.Counted));await LoadAsync();}catch(Exception ex){MessageBox.Show(ex.Message,"Stock",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
}

public sealed class MovementDialog:Form
{
    readonly ComboBox product=new(){DropDownStyle=ComboBoxStyle.DropDownList};readonly ComboBox type=new(){DropDownStyle=ComboBoxStyle.DropDownList};readonly NumericUpDown quantity=new(){Maximum=1000000,Minimum=1};readonly TextBox reason=new();readonly NumericUpDown current=new(){Maximum=1000000};readonly NumericUpDown counted=new(){Maximum=1000000};readonly IReadOnlyList<Produit> items;public Guid ProductId=>product.SelectedValue is Guid id?id:Guid.Empty;public string Type=>(string)type.SelectedItem!;public int Quantity=>(int)quantity.Value;public string Reason=>reason.Text.Trim();public int Current=>(int)current.Value;public int Counted=>(int)counted.Value;
    public MovementDialog(IReadOnlyList<Produit> items){this.items=items;Text="Mouvement de stock";Width=500;Height=380;StartPosition=FormStartPosition.CenterParent;var p=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(18),ColumnCount=2,RowCount=7};p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,40));p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,60));p.Controls.Add(new Label{Text="Produit",Anchor=AnchorStyles.Left,AutoSize=true},0,0);product.DataSource=items.ToList();product.DisplayMember="Nom";product.ValueMember="Id";p.Controls.Add(product,1,0);p.Controls.Add(new Label{Text="Nature",Anchor=AnchorStyles.Left,AutoSize=true},0,1);type.Items.AddRange(new object[]{TypesMouvement.StockInitial,TypesMouvement.Reapprovisionnement,TypesMouvement.Correction});type.SelectedIndex=0;p.Controls.Add(type,1,1);Add(p,"Quantité",quantity,2);Add(p,"Quantité actuelle",current,3);Add(p,"Quantité comptée",counted,4);Add(p,"Motif",reason,5);var b=Ui.Button("Enregistrer");b.Click+=(_,_)=>{if(string.IsNullOrWhiteSpace(Reason)){MessageBox.Show("Le motif est obligatoire.");return;}DialogResult=DialogResult.OK;Close();};p.Controls.Add(b,0,6);p.SetColumnSpan(b,2);Controls.Add(p);}
    static void Add(TableLayoutPanel p,string label,Control c,int row){p.Controls.Add(new Label{Text=label,Anchor=AnchorStyles.Left,AutoSize=true},0,row);c.Dock=DockStyle.Fill;p.Controls.Add(c,1,row);}
}

public sealed class InventoryPanel : UserControl
{
    readonly ProduitService products;readonly StockService stock;readonly DataGridView grid=Ui.Grid();public InventoryPanel(Utilisateur user,ProduitService products,StockService stock){this.products=products;this.stock=stock;Controls.Add(grid);Controls.Add(new Label{Text="Inventaire et état des stocks",Dock=DockStyle.Top,Height=52,Font=new Font("Segoe UI",18,FontStyle.Bold)});Load+=async(_,_)=>await LoadAsync();}
    async Task LoadAsync(){var rows=new List<object>();foreach(var p in await products.RechercherAsync(new ProduitFilter(Actif:null))) {var q=await stock.ObtenirStockAsync(p.Id);rows.Add(new{p.Reference,p.Nom,Stock=q,p.SeuilAlerte,Etat=Ui.StockLabel(q,p.SeuilAlerte)});}grid.DataSource=rows;foreach(DataGridViewRow row in grid.Rows){var value=row.Cells[4].Value?.ToString();row.DefaultCellStyle.BackColor=value=="RUPTURE"?Color.MistyRose:value=="STOCK FAIBLE"?Color.LemonChiffon:Color.Honeydew;}}
}

public sealed class UsersPanel : UserControl
{
    readonly Utilisateur actor;readonly UtilisateurService service;readonly DataGridView grid=Ui.Grid();public UsersPanel(Utilisateur actor,UtilisateurService service){this.actor=actor;this.service=service;var top=new FlowLayoutPanel{Dock=DockStyle.Top,Height=62};top.Controls.Add(Ui.Title("Utilisateurs"));var b=Ui.Button("Nouvel utilisateur");b.Click+=async(_,_)=>await AddAsync();top.Controls.Add(b);Controls.Add(grid);Controls.Add(top);Load+=async(_,_)=>await LoadAsync();}
    async Task LoadAsync(){grid.DataSource=(await service.ListerAsync(actor)).Select(x=>new{x.Id,x.NomUtilisateur,x.NomComplet,x.Role,Statut=x.Actif?"ACTIF":"DÉSACTIVÉ"}).ToList();}
    async Task AddAsync(){using var f=new UserDialog();if(f.ShowDialog()!=DialogResult.OK)return;try{await service.CreerAsync(actor,new Utilisateur(Guid.NewGuid(),f.Login,f.NameValue,f.Role),f.Password);await LoadAsync();}catch(Exception ex){MessageBox.Show(ex.Message,"Utilisateur",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
}
public sealed class UserDialog:Form
{
    readonly TextBox login=new(),name=new(),password=new();readonly ComboBox role=new(){DropDownStyle=ComboBoxStyle.DropDownList};public string Login=>login.Text.Trim();public string NameValue=>name.Text.Trim();public string Password=>password.Text;public string Role=>(string)role.SelectedItem!;
    public UserDialog(){Text="Nouvel utilisateur";Width=430;Height=300;StartPosition=FormStartPosition.CenterParent;var p=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(18),ColumnCount=2,RowCount=5};p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,35));p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,65));Add(p,"Identifiant",login,0);Add(p,"Nom",name,1);password.UseSystemPasswordChar=true;Add(p,"Mot de passe",password,2);p.Controls.Add(new Label{Text="Rôle",Anchor=AnchorStyles.Left,AutoSize=true},0,3);role.Items.AddRange(new object[]{Roles.Vendeur,Roles.Administrateur});role.SelectedIndex=0;p.Controls.Add(role,1,3);var b=Ui.Button("Créer");b.Click+=(_,_)=>{DialogResult=DialogResult.OK;Close();};p.Controls.Add(b,0,4);p.SetColumnSpan(b,2);Controls.Add(p);}
    static void Add(TableLayoutPanel p,string l,TextBox b,int r){p.Controls.Add(new Label{Text=l,Anchor=AnchorStyles.Left,AutoSize=true},0,r);b.Dock=DockStyle.Fill;p.Controls.Add(b,1,r);}
}
