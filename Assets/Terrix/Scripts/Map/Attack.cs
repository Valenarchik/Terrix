namespace Terrix.Map
{
    public class Attack
    {
        public Country AttackingCountry { get; }
        public Territory AttackedTerritory { get; }

        public Attack(Country attackingCountry, Territory attackedTerritory)
        {
            AttackingCountry = attackingCountry;
            AttackedTerritory = attackedTerritory;
        }
    }
}