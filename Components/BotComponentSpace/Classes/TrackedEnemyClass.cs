using SAIN.SAINComponent.Classes.EnemyClasses;
using System;

namespace SAIN.SAINComponent.Classes.Search
{
    public class TrackedEnemyClass : BotBase, IBotClass
    {
        public event Action<Enemy, Enemy> OnNewEnemySet;

        public Enemy Enemy { get; private set; }

        public TrackedEnemyClass(BotComponent bot) : base(bot)
        {
        }

        public void Init()
        {
            Bot.EnemyController.Events.OnEnemyRemoved += checkClearEnemy;
            Bot.EnemyController.Events.OnEnemyChanged += enemyChanged;
        }

        public void Update()
        {
            if (!Bot.Search.SearchActive) {
                if (Enemy != null) {
                    Clear();
                }
                return;
            }
            updateEnemy();
        }

        public void Dispose()
        {
            Bot.EnemyController.Events.OnEnemyRemoved -= checkClearEnemy;
            Bot.EnemyController.Events.OnEnemyChanged -= enemyChanged;
        }

        private void checkClearEnemy(string profileId, Enemy enemy)
        {
            if (Enemy == null) {
                return;
            }
            if (Enemy.EnemyProfileId == profileId) {
                Clear();
            }
        }

        private void enemyChanged(Enemy enemy, Enemy lastEnemy)
        {
            if (Enemy == null) {
                return;
            }
            Clear();
            if (enemy != null) setTarget(enemy);
        }

        public void Clear()
        {
            OnNewEnemySet?.Invoke(null, Enemy);
            Enemy = null;
        }

        private void updateEnemy()
        {
            if (Enemy != null &&
                (!Enemy.EnemyKnown ||
                !Enemy.Person.Active ||
                !Enemy.WasValid)) {
                Clear();
            }
            if (Enemy == null) {
                var activeEnemy = Bot.Enemy;
                if (activeEnemy == null) return;
                setTarget(activeEnemy);
            }
        }

        private void setTarget(Enemy enemy)
        {
            OnNewEnemySet?.Invoke(enemy, Enemy);
            Enemy = enemy;
        }

        public void Start()
        {
            setTarget(Bot.Enemy);
        }

        public void Stop()
        {
            Clear();
        }
    }
}