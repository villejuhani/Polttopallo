using Jypeli;
using Jypeli.Assets;
using System;
using System.Collections.Generic;

/// @author Ville Hytönen
/// @version 3.2020
/// <summary>
/// Polttopallo -peli
/// </summary>
public class Polttopallo : PhysicsGame
{
    public override void Begin()
    {
        Level.Background.Color = Color.Black;
        Camera.ZoomToLevel();
        SetWindowSize(1024, 768);

        PhysicsObject pelaaja = new PhysicsObject(30, 30, Shape.Circle);
        pelaaja.LinearDamping = 0.8;
        pelaaja.Restitution = 0.0;
        Add(pelaaja);

        Ohjaimet(pelaaja);

        LuoHakki(400, 10, Shape.Rectangle, 0, 200);
        LuoHakki(400, 10, Shape.Rectangle, 0, -200);
        LuoHakki(10, 400, Shape.Rectangle, 200, 0);
        LuoHakki(10, 400, Shape.Rectangle, -200, 0);

        GameObject lattia = new GameObject(400, 400);
        lattia.Image = LoadImage("lattiasun");
        Add(lattia, -1);

        Timer.CreateAndStart(0.4, LuoPolttopallo);
        AddCollisionHandler(pelaaja, "polttopallo", CollisionHandler.DestroyObject);
    }


    /// <summary>
    /// aliohjelma asetaa pelaajalle ohjaimet
    /// </summary>
    /// <param name="pelaaja">olio, jolle ohjaimet asetetaan</param>
    public void Ohjaimet(PhysicsObject pelaaja)
    {
        Keyboard.Listen(Key.Left, ButtonState.Down, delegate ()
        {
            pelaaja.Push(new Vector(-3000, 0));
        }, "Liikuta pelaajaa vasemmalle");
        Keyboard.Listen(Key.Left, ButtonState.Released, Liikuta, null, pelaaja, Vector.Zero);

        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikuta pelaajaa oikealle", pelaaja, new Vector(3000, 0));
        Keyboard.Listen(Key.Right, ButtonState.Released, Liikuta, null, pelaaja, Vector.Zero);

        Keyboard.Listen(Key.Down, ButtonState.Down, Liikuta, "Liikuta pelaajaa alas", pelaaja, new Vector(0, -3000));
        Keyboard.Listen(Key.Down, ButtonState.Released, Liikuta, null, pelaaja, Vector.Zero);

        Keyboard.Listen(Key.Up, ButtonState.Down, Liikuta, "Liikuta pelaajaa ylös", pelaaja, new Vector(0, 3000));
        Keyboard.Listen(Key.Up, ButtonState.Released, Liikuta, null, pelaaja, Vector.Zero);

        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }


    /// <summary>
    /// luo pelaajalle liikkumisen fysiikan
    /// </summary>
    /// <param name="pelaaja">olio, jonka liikkumista määritetään</param>
    /// <param name="suunta">liikkumis suunta</param>
    public void Liikuta(PhysicsObject pelaaja, Vector suunta)
    {
        pelaaja.Push(suunta);
    }


    /// <summary>
    /// aliohjelma luo yhden häkin sivun
    /// </summary>
    /// <param name="width">sivun leveys</param>
    /// <param name="height">korkeus</param>
    /// <param name="shape">muoto</param>
    /// <param name="sijaintiX">sijainnin x-koordinaatti</param>
    /// <param name="sijaintiY">sijainnin y-koordinaatti</param>
    /// <returns>häkin seinä</returns>
    PhysicsObject LuoHakki(double width, double height, Shape shape, double sijaintiX, double sijaintiY)
    {
        PhysicsObject hakki = new PhysicsObject(width, height, shape, sijaintiX, sijaintiY);
        hakki.Color = Color.DarkBrown;
        hakki.X = sijaintiX;
        hakki.Y = sijaintiY;
        hakki.MakeStatic();
        hakki.CollisionIgnoreGroup = 1;
        Add(hakki, -1);
        return hakki;
    }


    /// <summary>
    /// Aliohjelma luo polttopallon ja sen liikeradan
    /// </summary>
    public void LuoPolttopallo()
    {
        PhysicsObject polttopallo = new PhysicsObject(30, 30, Shape.Circle);
        polttopallo.Color = Color.BloodRed;
        polttopallo.CollisionIgnoreGroup = 1;
        List<Vector> paikka = new List<Vector>();
        polttopallo.Position = LuoPaikka();

        PathFollowerBrain polkuAivot = new PathFollowerBrain();
        List<Vector> polku = new List<Vector>();
        if (polttopallo.Y < -399) polku.Add(new Vector(0, 450)); //polku ylös
        if (polttopallo.Y > 399) polku.Add(new Vector(0, -450)); //alas
        if (polttopallo.X < -399) polku.Add(new Vector(650, 0)); //oikealle
        if (polttopallo.X > 399) polku.Add(new Vector(-650, 0)); //vasemmalle

        polkuAivot.Path = polku;
        polkuAivot.Speed = 200;
        polttopallo.Brain = polkuAivot;
        polttopallo.LifetimeLeft = TimeSpan.FromSeconds(10.0);

        PhysicsObject polttopallot = polttopallo;
        polttopallo.Tag = "polttopallo";
        Add(polttopallo);
    }


    /// <summary>
    /// Aliohjelma arpoo listasta alueen, jolle polttopallo luodaan
    /// </summary>
    /// <returns>polttopallon koordinaatit</returns>
    public Vector LuoPaikka()
    {
        var random = new Random();
        List<Vector> lista = new List<Vector>();
        lista.Add(RandomGen.NextVector(-400, -700, 400, -400)); //paikka alas
        lista.Add(RandomGen.NextVector(-400, 400, 400, 800)); // ylös
        lista.Add(RandomGen.NextVector(400, -400, 700, 400)); //oikealle
        lista.Add(RandomGen.NextVector(-400, -700, -800, 400)); //vasemmalle

        int paikka = random.Next(lista.Count);
        return (lista[paikka]);
    }


}

