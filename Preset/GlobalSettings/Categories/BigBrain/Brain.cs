using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings.Categories
{
    public enum Brain
    {
        ArenaFighter,
        BossBully,
        BossGluhar,
        Knight,
        BossKojaniy,
        BossSanitar,
        Tagilla,
        BossTest,
        //BossZryachiy,
        //PeaceZryachiy,
        //RavangeZryachiy,
        Obdolbs,
        ExUsec,
        BigPipe,
        BirdEye,
        FollowerBully,
        FollowerGluharAssault,
        FollowerGluharProtect,
        FollowerGluharScout,
        FollowerKojaniy,
        FollowerSanitar,
        TagillaFollower,
        //Fl_Zraychiy,
        Gifter,
        Killa,
        Marksman,
        PMC,
        SectantPriest,
        SectantWarrior,
        CursAssault,
        Assault,
        BossBoar,
        FlBoarSt,
        FlBoarCl,
        BoarSniper,
        BossKolontay,
        FlKlnAslt,
        KolonSec,
        BossPartisan,
        //SctPredvst,
        //PrizrakSt,
        //Oni,
        //InfectedFast,
    }

    public static class AIBrains
    {
        public static readonly List<Brain> Scavs = new List<Brain>
        {
            Brain.CursAssault,
            Brain.Assault,
        };

        public static readonly List<Brain> Goons = new List<Brain>
        {
            Brain.Knight,
            Brain.BirdEye,
            Brain.BigPipe,
        };

        public static readonly List<Brain> Others = new List<Brain>
        {
            Brain.Obdolbs,
        };

        public static readonly List<Brain> Bosses = new List<Brain>
        {
            Brain.BossBully,
            Brain.BossGluhar,
            Brain.BossKojaniy,
            Brain.BossSanitar,
            Brain.Tagilla,
            Brain.BossTest,
            //Brain.BossZryachiy,
            Brain.Gifter,
            Brain.Killa,
            Brain.SectantPriest,
            Brain.BossBoar,
            Brain.BossKolontay,
            Brain.BossPartisan,
        };

        public static readonly List<Brain> Followers = new List<Brain>
        {
            Brain.FollowerBully,
            Brain.FollowerGluharAssault,
            Brain.FollowerGluharProtect,
            Brain.FollowerGluharScout,
            Brain.FollowerKojaniy,
            Brain.FollowerSanitar,
            Brain.TagillaFollower,
            //Brain.Fl_Zraychiy,
            Brain.FlBoarSt,
            Brain.FlBoarCl,
            Brain.BoarSniper,
            Brain.FlKlnAslt,
            Brain.KolonSec,
        };
    }
}