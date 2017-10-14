using System.ComponentModel.DataAnnotations;

namespace blockchain_parser.Model
{
    public abstract class VersionedRow
    {
        [ConcurrencyCheck]
        public abstract long RowVersion {get; set;}

        public void OnSavingChanges()
        {
            RowVersion++;
        }
    }
}