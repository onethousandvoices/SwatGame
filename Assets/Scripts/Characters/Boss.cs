using NTC.Global.Pool;

namespace SWAT
{
    public class Boss : BaseCharacter, IPoolItem
    {
        public override CharacterType Type => CharacterType.Boss;
        protected override int BaseMaxArmour { get; }
        protected override int BaseMaxHealth { get; }
        
        public void OnSpawn()
        {
            throw new System.NotImplementedException();
        }

        public void OnDespawn()
        {
            throw new System.NotImplementedException();
        }
    }
}