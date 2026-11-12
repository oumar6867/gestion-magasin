namespace GestionMagasin.Domain;
public sealed class PrixProduitModifieException(string message) : DomainException(message);
public sealed class StockInsuffisantException(string message) : DomainException(message);
