using Jypeli;

/// @author Ville Hytönen
/// @version 17.4.2020
/// <summary>
/// PhysicsObject elämillä
/// </summary>
class PelattavaHahmo : PhysicsObject
{
    private IntMeter elamaLaskuri = new IntMeter(3, 0, 3);
    public IntMeter ElamaLaskuri { get { return elamaLaskuri; } }
    public PelattavaHahmo(double leveys, double korkeus, Shape shape)
        : base(leveys, korkeus)
    {
        elamaLaskuri.LowerLimit += delegate { this.Destroy(); };
    }


    /// <summary>
    /// Paluattaa olion elämien määrän
    /// </summary>
    /// <returns>elämät</returns>
    public int Elamat()
    {
        return elamaLaskuri.Value;
    }


    /// <summary>
    /// Laskee olion elämien määrää
    /// </summary>
    public void Osuma()
    {
        elamaLaskuri.Value--;
    }


    /// <summary>
    /// Nostaa olion elämien määrää
    /// </summary>
    public void TerveellinenOsuma()
    {
        elamaLaskuri.Value++;
    }


}

