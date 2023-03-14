namespace Diplomka.Models.Abstract
{
    public abstract class Entity : IEntity
    {
        public Guid Id { get; set; }
        public DateTime DateCreated { get; set; }

        public Entity()
        {
            DateCreated = DateTime.UtcNow;
        }
    }

}
