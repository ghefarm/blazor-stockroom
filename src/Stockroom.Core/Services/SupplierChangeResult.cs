namespace Stockroom.Core.Services;

// What a change of suppliers did. The page turns it into a message,
// so the service never needs to know any text of the user interface.
public enum SupplierChangeResult
{
    Done,
    NotFound,
    NameTaken,
}
