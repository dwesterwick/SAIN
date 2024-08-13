namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public class CoverFromEnemyClass : EnemyBase, IBotEnemyClass
    {
        public VisionCoverFromEnemy VisionCover { get; }

        public CoverFromEnemyClass(Enemy enemy) : base(enemy)
        {
            VisionCover = new VisionCoverFromEnemy(enemy);
        }

        public void Init()
        {
            //Enemy.Events.OnEnemyKnownChanged.OnToggle += OnEnemyKnownChanged;
            VisionCover.Init();
        }

        public void Update()
        {
            VisionCover.Update();
        }

        public void Dispose()
        {
            //Enemy.Events.OnEnemyKnownChanged.OnToggle -= OnEnemyKnownChanged;
            VisionCover.Dispose();
        }

        public void OnEnemyKnownChanged(bool known, Enemy enemy)
        {
        }
    }
}