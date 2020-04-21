using Jypeli;
using Jypeli.Assets;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;

/// @author Ville Hytönen
/// @version 17.4.2020
/// <summary>
/// Polttopallopeli, väistellään kohti tulevia "polttopalloja"
/// </summary>
public class Polttopallo : PhysicsGame
{
    // TODO: taulukko/lista, ks: https://tim.jyu.fi/answers/kurssit/tie/ohj1/2020k/demot/demo11?answerNumber=7&task=bussipysakit&user=hytovjxz
    private PelattavaHahmo pelaaja;
    private PelattavaHahmo pelaaja2;
    private Widget sydamet;
    private Widget sydametPelaaja2;
    private int pelaajienMaara = 1;
    private bool kaksinPeli = false;
    private Timer aikaLaskuri = new Timer();
    private EasyHighScore topLista = new EasyHighScore();
    private int vihunNopeus;

    private const int SEINIEN_PITUUS = 400;
    private const int HAKKI_POSITIIVINEN = SEINIEN_PITUUS / 2;
    private const int HAKKI_NEGATIIVINEN = SEINIEN_PITUUS / 2 * -1;


    /// <summary>
    /// luodaan alkuvalikko ja asetetaan peli-ikkunan koko
    /// </summary>
    public override void Begin()
    {
        Level.Background.Color = Color.Black;

        SetWindowSize(1024, 768);
        //SetWindowPosition(2300, 150); //peli näkymään päänäytölleni

        LuoAlkuValikko();
    }


    /// <summary>
    /// aliohjelma luo alkuvalikon
    /// </summary>
    private void LuoAlkuValikko()
    {
        MultiSelectWindow alkuValikko = new MultiSelectWindow("Polttopallo", "Yksinpeli", "Kaksinpeli", "Lopeta peli");
        alkuValikko.AddItemHandler(0, AloitaPeli);
        alkuValikko.AddItemHandler(1, KaksinPeli);
        alkuValikko.AddItemHandler(2, Exit);
        alkuValikko.SelectionColor = Color.BlueGray;
        alkuValikko.BorderColor = Color.Gray;
        alkuValikko.Image = LoadImage("lattiasun");
        Add(alkuValikko);
    }


    /// <summary>
    /// aliohjelma aloittaa pelin
    /// </summary>
    private void AloitaPeli()
    {
        if (IsPaused) Pause();
        Camera.ZoomToLevel();

        pelaaja = LuoPelaaja();
        AddCollisionHandler<PelattavaHahmo, PhysicsObject>(pelaaja, "kerattava", TormaysKerattaviin);
        AddCollisionHandler<PelattavaHahmo, PhysicsObject>(pelaaja, "keihas", TormaysVihuun);
        Ohjaimet(pelaaja, Key.Left, Key.Right, Key.Down, Key.Up);

        LuoHakki();
        LuoLattia();

        vihunNopeus = 200;
        NopeutaKeihaita();
        LuoKeihaita();

        Timer.CreateAndStart(7, LuoKerattava);
        sydamet = LuoSydamet(pelaaja.Elamat(), Color.Red);

        LuoAjastin();
        topLista.HighScoreWindow.NameInputWindow.Message.Text = "TOP 10 Babyyy!! {0:0.0} sekuntia!";
        topLista.HighScoreWindow.List.ScoreFormat = "{0:0.0}";
    }


    /// <summary>
    /// luodaan toinen pelattava hahmo
    /// </summary>
    private void KaksinPeli()
    {
        kaksinPeli = true;
        pelaajienMaara++;
        AloitaPeli();
        pelaaja2 = LuoPelaaja();
        AddCollisionHandler<PelattavaHahmo, PhysicsObject>(pelaaja2, "kerattava", TormaysKerattaviin);
        AddCollisionHandler<PelattavaHahmo, PhysicsObject>(pelaaja2, "keihas", TormaysVihuun);
        Ohjaimet(pelaaja2, Key.A, Key.D, Key.S, Key.W);
        pelaaja2.X = -40;
        pelaaja2.Image = LoadImage("scaredbro");
        sydametPelaaja2 = LuoSydamet(pelaaja2.Elamat(), Color.LightBlue, 30);
    }


    /// <summary>
    /// aliohjelma luo pelaajan
    /// </summary>
    private PelattavaHahmo LuoPelaaja()
    {
        PelattavaHahmo pelaaja = new PelattavaHahmo(50, 50, Shape.Circle);
        pelaaja.LinearDamping = 0.8;
        pelaaja.Restitution = 0.0;
        pelaaja.CanRotate = false;
        pelaaja.Image = LoadImage("bro");
        pelaaja.CollisionIgnoreGroup = 2;
        Add(pelaaja);
        return pelaaja;
    }


    /// <summary>
    /// aliohjelma luo ohjaimet pelaajille
    /// </summary>
    /// <param name="pelaaja">pelattava hahmo</param>
    /// <param name="vasen">näppäin</param>
    /// <param name="oikea">näppäin</param>
    /// <param name="alas">näppäin</param>
    /// <param name="ylos">näppäin</param>
    private void Ohjaimet(PhysicsObject pelaaja, Key vasen, Key oikea, Key alas, Key ylos)
    {
        Keyboard.Listen(vasen, ButtonState.Down, Liikuta, "Liikuta pelaajaa vasemmalle", pelaaja, new Vector(-3000, 0));
        Keyboard.Listen(vasen, ButtonState.Released, Liikuta, null, pelaaja, Vector.Zero);

        Keyboard.Listen(oikea, ButtonState.Down, Liikuta, "Liikuta pelaajaa oikealle", pelaaja, new Vector(3000, 0));
        Keyboard.Listen(oikea, ButtonState.Released, Liikuta, null, pelaaja, Vector.Zero);

        Keyboard.Listen(alas, ButtonState.Down, Liikuta, "Liikuta pelaajaa alas", pelaaja, new Vector(0, -3000));
        Keyboard.Listen(alas, ButtonState.Released, Liikuta, null, pelaaja, Vector.Zero);

        Keyboard.Listen(ylos, ButtonState.Down, Liikuta, "Liikuta pelaajaa ylös", pelaaja, new Vector(0, 3000));
        Keyboard.Listen(ylos, ButtonState.Released, Liikuta, null, pelaaja, Vector.Zero);

        Keyboard.Listen(Key.R, ButtonState.Pressed, AloitaPeliUudelleen, "aloittaa pelin alusta");
        Keyboard.Listen(Key.T, ButtonState.Pressed, TakaisinAlkuValikkoon, "palaa alkuvalikkoon");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }


    /// <summary>
    /// luo pelaajalle liikkumisen fysiikan
    /// </summary>
    /// <param name="pelaaja">olio, jonka liikkumista määritetään</param>
    /// <param name="suunta">liikkumis suunta</param>
    private void Liikuta(PhysicsObject pelaaja, Vector suunta)
    {
        pelaaja.Push(suunta);
    }


    /// <summary>
    /// Aliohjelma luo nelisivuisen häkin
    /// </summary>
    /// <returns>Häkki</returns>
    private void LuoHakki()
    {
        LuoHakki(SEINIEN_PITUUS, 10, Shape.Rectangle, 0, HAKKI_POSITIIVINEN); //yläseinä
        LuoHakki(SEINIEN_PITUUS, 10, Shape.Rectangle, 0, HAKKI_NEGATIIVINEN); //alaseinä
        LuoHakki(10, SEINIEN_PITUUS, Shape.Rectangle, HAKKI_POSITIIVINEN, 0); //oikeaseinä
        LuoHakki(10, SEINIEN_PITUUS, Shape.Rectangle, HAKKI_NEGATIIVINEN, 0); //vasenseinä
    }


    /// <summary>
    /// aliohjelma luo yhden häkin sivun
    /// </summary>
    /// <param name="width">sivun leveys</param>
    /// <param name="height">korkeus</param>
    /// <param name="shape">muoto</param>
    /// <param name="sijaintiX">sijainnin x-koordinaatti</param>
    /// <param name="sijaintiY">sijainnin y-koordinaatti</param>
    /// <returns>häkin sivu</returns>
    private void LuoHakki(double width, double height, Shape shape, double sijaintiX, double sijaintiY)
    {
        PhysicsObject hakki = new PhysicsObject(width, height, shape, sijaintiX, sijaintiY);
        hakki.Color = Color.DarkBrown;
        hakki.X = sijaintiX;
        hakki.Y = sijaintiY;
        hakki.MakeStatic();
        hakki.CollisionIgnoreGroup = 1;
        Add(hakki, -1);
    }


    /// <summary>
    /// Aliohjelma luo upean lattian jolla käydään tanssi elämästä ja kuolemasta
    /// </summary>
    /// <returns>lattia</returns>
    private GameObject LuoLattia()
    {
        GameObject lattia = new GameObject(SEINIEN_PITUUS, SEINIEN_PITUUS);
        lattia.Image = LoadImage("lattiasun");
        Add(lattia, -1);
        return lattia;
    }


    /// <summary>
    /// Hiljalleen kasvatetaan vihunNopeus attribuuttia
    /// </summary>
    private void NopeutaKeihaita()
    {
        Timer vihunNopeutusAjastin = new Timer();
        vihunNopeutusAjastin.Interval = 5;
        vihunNopeutusAjastin.Timeout += delegate
        {
            vihunNopeus += 20;
        };
        vihunNopeutusAjastin.Start();
    }


    /// <summary>
    /// Luodaan ajastin joka hiljalleen nopeuttaa keihäiden luontia ja kutsutaan LuoKeihas
    /// aliohjelmaa eli myös luodaan keihäät tätä kautta
    /// </summary>
    private void LuoKeihaita()
    {
        Timer vihunLuonti = Timer.CreateAndStart(0.4, LuoKeihas);

        Timer olioidenSynnyttamisenNopeutin = new Timer();
        olioidenSynnyttamisenNopeutin.Interval = 10;
        olioidenSynnyttamisenNopeutin.Timeout += delegate
        {
            vihunLuonti.Interval -= 0.035;
        };
        olioidenSynnyttamisenNopeutin.Start();
    }


    /// <summary>
    /// Aliohjelma luo keihään, sijoittaa sen ja luo sen liikeradan
    /// </summary>
    private void LuoKeihas()
    {
        PhysicsObject keihas = new PhysicsObject(30, 80, Shape.Ellipse);
        keihas.Color = Color.BloodRed;
        keihas.CollisionIgnoreGroup = 1;
        keihas.Image = LoadImage("keihas");

        int keihaanLuontiEtaisyys = 600;
        keihas.Position = keihaanLuontiEtaisyys * Vector.FromAngle(RandomGen.NextAngle());
        Vector pisteHakinSisalta = new Vector(RandomGen.NextInt(HAKKI_NEGATIIVINEN, HAKKI_POSITIIVINEN), RandomGen.NextInt(HAKKI_NEGATIIVINEN, HAKKI_POSITIIVINEN));
        Vector lopullinen = pisteHakinSisalta - keihas.Position;
        lopullinen = lopullinen.Normalize() * keihaanLuontiEtaisyys * 2;
        List<Vector> polku = new List<Vector>();
        polku.Add(lopullinen);

        PathFollowerBrain polkuAivot = new PathFollowerBrain();
        polkuAivot.Path = polku;
        polkuAivot.Speed = vihunNopeus;
        polkuAivot.ArrivedAtEnd += keihas.Destroy;
        keihas.LifetimeLeft = TimeSpan.FromSeconds(7.0);
        keihas.Brain = polkuAivot;

        keihas.Angle = lopullinen.Angle - Angle.RightAngle;
        keihas.Tag = "keihas";
        Add(keihas);
    }


    /// <summary>
    /// luodaan kerattava olio, josta pelattavahahmo saa elämiä ja luodaan se varmasti ainakin pienen välimatkan päähän pelaajista
    /// </summary>
    private void LuoKerattava()
    {
        PhysicsObject kerattava = new PhysicsObject(25, 25, Shape.Heart);
        kerattava.Color = Color.Magenta;
        Vector nolla = new Vector(0, 0);
        Vector minMatkaPelaajasta = new Vector(0, 150);

        bool kauaksiPelaajista = false;
        do
        {
            kerattava.Position = new Vector(RandomGen.NextInt(HAKKI_NEGATIIVINEN + 5, HAKKI_POSITIIVINEN - 5), RandomGen.NextInt(HAKKI_NEGATIIVINEN + 5, HAKKI_POSITIIVINEN - 5));
            if (kaksinPeli) kauaksiPelaajista = Vector.Distance(kerattava.Position, pelaaja2.Position) < Vector.Distance(nolla, minMatkaPelaajasta);

        } while (Vector.Distance(kerattava.Position, pelaaja.Position) < Vector.Distance(nolla, minMatkaPelaajasta) || kauaksiPelaajista);

        kerattava.Tag = "kerattava";
        kerattava.CollisionIgnoreGroup = 1;
        Add(kerattava);
    }


    /// <summary>
    /// luodaan törmäystapahtumat kerattaviin osuttaessa
    /// </summary>
    /// <param name="pelattava"></param>
    /// <param name="kerattava"></param>
    private void TormaysKerattaviin(PelattavaHahmo pelattava, PhysicsObject kerattava)
    {
        pelattava.TerveellinenOsuma();
        kerattava.Destroy();
        UudetSydamet(pelattava);
    }


    /// <summary>
    /// luodaan törmäystapahtumat polttopalloihin osuttaessa
    /// </summary>
    /// <param name="pelattava"></param>
    /// <param name="keihas"></param>
    private void TormaysVihuun(PelattavaHahmo pelattava, PhysicsObject keihas)
    {
        pelattava.Osuma();
        keihas.Destroy();
        UudetSydamet(pelattava);

        if (pelattava.IsDestroyed)
        {
            pelaajienMaara--;
        }
        if (pelaajienMaara == 0)
        {
            Pause();
            LuoApuTekstit("R - Aloita alusta", 0);
            LuoApuTekstit("T - Alkuvalikko", -30);
            LuoApuTekstit("ESC - Poistu pelistä", -60);

            topLista.EnterAndShow(aikaLaskuri.CurrentTime);
        }
    }


    /// <summary>
    /// Selvitetään mikä pelaaja törmäsi ja muutetaan kyseisen pelaajan sydämet
    /// vastaamaan pelaajan elämiä
    /// </summary>
    /// <param name="pelattava"></param>
    private void UudetSydamet(PelattavaHahmo pelattava)
    {
        if (pelattava == pelaaja)
        {
            sydamet.Clear();
            sydamet = LuoSydamet(pelattava.Elamat(), Color.Red);
        }
        else
        {
            sydametPelaaja2.Clear();
            sydametPelaaja2 = LuoSydamet(pelattava.Elamat(), Color.LightBlue, 30);
        }
    }


    /// <summary>
    /// Luodaan pelaajalle visuaalinen representaatio pelaajan elämistä
    /// </summary>
    /// <param name="pelaajanTerveys">pelaajan elämät</param>
    /// <param name="vari">sydänten väri</param>
    /// <returns>sydämet</returns>
    public Widget LuoSydamet(int pelaajanTerveys, Color vari)
    {
        return LuoSydamet(pelaajanTerveys, vari, 0);
    }


    /// <summary>
    /// Luodaan pelaajalle visuaalinen representaatio pelaajan elämistä
    /// </summary>
    /// <param name="pelaajanTerveys">pelaajan elämät</param>
    /// <param name="vari">sydänten väri</param>
    /// <param name="y">Y-koordinaattiin tehtävä lisäys</param>
    /// <returns>sydämet</returns>
    public Widget LuoSydamet(int pelaajanTerveys, Color vari, int y)
    {
        HorizontalLayout asettelu = new HorizontalLayout();
        asettelu.Spacing = 10;

        Widget sydamet = new Widget(asettelu);
        sydamet.Color = Color.Transparent;
        sydamet.X = HAKKI_NEGATIIVINEN + 50;
        sydamet.Y = HAKKI_POSITIIVINEN + 20 + y;
        Add(sydamet);

        for (int i = 0; i < pelaajanTerveys; i++)
        {
            Widget sydan = new Widget(25, 25, Shape.Heart);
            sydan.Color = vari;
            sydamet.Add(sydan);
        }
        return sydamet;
    }


    /// <summary>
    /// aliohjelma luo ajastimen
    /// </summary>
    private void LuoAjastin()
    {
        aikaLaskuri = new Timer();
        aikaLaskuri.Start();

        Label naytto = new Label();
        naytto.TextColor = Color.White;
        naytto.DecimalPlaces = 1;
        naytto.BindTo(aikaLaskuri.SecondCounter);
        naytto.Position = new Vector(HAKKI_POSITIIVINEN - 20, HAKKI_POSITIIVINEN + 15);
        Add(naytto);
    }


    /// <summary>
    /// luodaan aputekstit pelaajan kuoltua
    /// </summary>
    /// <param name="teksti">aputeksti</param>
    /// <param name="paikka">aputekstin paikka</param>
    private void LuoApuTekstit(string teksti, double paikka)
    {
        Label uudelleenaloitus = new Label(teksti);
        uudelleenaloitus.X = HAKKI_POSITIIVINEN + 120;
        uudelleenaloitus.Y = paikka;
        uudelleenaloitus.TextColor = Color.White;
        Add(uudelleenaloitus);
    }


    /// <summary>
    /// Luodaan toiminnot, jotka tapahtuvat kun 'r' -näppäintä painetaan
    /// </summary>
    private void AloitaPeliUudelleen()
    {
        ClearAll();
        pelaajienMaara = 1;
        if (kaksinPeli) KaksinPeli();
        else AloitaPeli();
    }


    /// <summary>
    /// Luodaan toiminnot, jotka tapahtuvat kun 't' -näppäintä painetaan
    /// </summary>
    private void TakaisinAlkuValikkoon()
    {
        ClearAll();
        pelaajienMaara = 1;
        if (kaksinPeli) kaksinPeli = false;
        LuoAlkuValikko();
    }


}