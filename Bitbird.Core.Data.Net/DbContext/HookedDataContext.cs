using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bitbird.Core.Data.DbContext.Hooks;
using Bitbird.Core.Data.DbContext.Interceptors;
using Bitbird.Core.Data.Validation;
using Bitbird.Core.Tasks;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.DbContext
{
    /// <summary>
    /// Manages the database access.
    ///
    /// In addition to the normal entity framework functionality, this class adds the following functionality:
    /// - set <c>ARITHABORT ON</c> for every SQL-command.
    /// - enable database hooks, see <see cref="DataContextHookCollection{TDataContext, TState}"/>.
    /// - defining concurrency fields and their precision.
    /// 
    /// For more information about <see cref="DbContext"/> see the architecture of entity framework.
    /// </summary>
    /// <inheritdoc cref="DbContext" />
    public abstract class HookedStateDataContext<TDataContext, TState> 
        : System.Data.Entity.DbContext 
        , IHookedStateDataContext<TState>
        , IGetQueryByEntity
        where TDataContext : System.Data.Entity.DbContext
    {
        /// <summary>
        /// Stores the current hooks.
        /// This instance is already cloned from the hooks that were passed to the constructor, and therefore can be modified.
        /// Can be null.
        /// </summary>
        [CanBeNull]
        private readonly DataContextHookCollection<TDataContext, TState> hooks;


        protected virtual void OnPreHook([NotNull] TDataContext db, [CanBeNull] TState state, [NotNull, ItemNotNull] EntityHookEvent<object>[] entities, HookEventType type)
        {
        }

        #region Constructor and instanciation

        /// <summary>
        /// Main constructor. All other constructors should call this one.
        /// For more information about <see cref="System.Data.Entity.DbContext" /> see the architecture of entity framework.
        /// </summary>
        /// <param name="nameOrConnectionString">The connection string to use for the connection to the database.</param>
        /// <param name="hooks">The database hooks object to use. Can be null if no hooks are required. The passed object will not be changed but cloned.</param>
        /// <inheritdoc />
        public HookedStateDataContext(
            [NotNull] string nameOrConnectionString, 
            [CanBeNull] DataContextHookCollection<TDataContext, TState> hooks)
            : base(nameOrConnectionString)
        {
            Database.CommandTimeout = 60;

            this.hooks = hooks?.Clone();
            this.hooks?.ForAll().AddPreEventAsyncHandler((db, state, entities, type) =>
            {
                if (type == HookEventType.Delete)
                    return Task.FromResult(false);

                OnPreHook(db, state, entities, type);

                return Task.FromResult(true);
            });

            Configuration.AutoDetectChangesEnabled = true;

            Database.SetInitializer<TDataContext>(null);
        }
        #endregion

        #region Internal functionality
        /// <summary>
        /// Internal functionality.
        /// Don't call from outside.
        /// Currently this method is public to make it easier to call it using reflection.
        /// Used during <see cref="OnModelCreating"/>, to configure entities.
        /// If this method returns true, no further configuration is done.
        /// </summary>
        /// <typeparam name="T">The entity type to configure.</typeparam>
        /// <param name="modelBuilder">The model builder that was passed to <see cref="OnModelCreating"/>.</param>
        /// <returns>true if no further processing should be done.</returns>
        [UsedImplicitly]
        public virtual bool ConfigureEntity<T>([NotNull] DbModelBuilder modelBuilder)
        {
            return false;
        }

        /// <summary>
        /// Internal functionality.
        /// Don't call from outside.
        /// Currently this method is public to make it easier to call it using reflection.
        /// Used during <see cref="OnModelCreating"/>, to configure all entities that implement <see cref="IOptimisticLockable"/>:
        /// - sets <see cref="IOptimisticLockable.SysStartTime"/> as concurrency token.
        /// - sets the SQL-type of <see cref="IOptimisticLockable.SysStartTime"/> to datetime2(0).
        /// </summary>
        /// <typeparam name="T">The entity type to configure.</typeparam>
        /// <param name="modelBuilder">The model builder that was passed to <see cref="OnModelCreating"/>.</param>
        [UsedImplicitly]
        public void ConfigureIOptimisticLockable<T>([NotNull] DbModelBuilder modelBuilder) 
            where T : class, IOptimisticLockable
        {
            var entity = modelBuilder.Entity<T>();
            entity.Property(x => x.SysStartTime)
                .HasColumnType("datetime2")
                .HasPrecision(0)
                .IsConcurrencyToken();
        }


        /// <inheritdoc />
        protected override void OnModelCreating([NotNull] DbModelBuilder modelBuilder)
        {
            var type = GetType();

            // get all entity types 
            var entityTypes = type
                .GetProperties()
                .Where(p => typeof(IQueryable).IsAssignableFrom(p.PropertyType))
                .Select(p => p.PropertyType.GetGenericArguments()[0])
                .ToArray();

            // Get generic method ConfigureEntity
            var methodConfigure = type.GetMethod(nameof(ConfigureEntity)) 
                                               ?? throw new Exception($"Could not find method {type.FullName}.{nameof(ConfigureEntity)}");
            // Get generic method ConfigureIOptimisticLockable
            var methodConfigureIOptimisticLockable = type.GetMethod(nameof(ConfigureIOptimisticLockable)) 
                                                     ?? throw new Exception($"Could not find method {type.FullName}.{nameof(ConfigureIOptimisticLockable)}");

            foreach (var entityType in entityTypes)
            {
                // Call typed methods

                var result = (bool)(methodConfigure.MakeGenericMethod(entityType).Invoke(this, new object[] {modelBuilder}));
                if (result)
                    continue;

                if (typeof(IOptimisticLockable).IsAssignableFrom(entityType))
                    methodConfigureIOptimisticLockable.MakeGenericMethod(entityType).Invoke(this, new object[] { modelBuilder });
            }

            base.OnModelCreating(modelBuilder);
        }


        [NotNull]
        public Task<int> SaveChangesAsync(TState state)
            => SaveChangesAsync(state, CancellationToken.None);

        [NotNull] 
        public async Task<int> SaveChangesAsync(TState state, CancellationToken cancellationToken)
        {
            // Add db-hook functionality (see class documentation).
            var hookInvoker = new DataContextHookInvoker<TDataContext, TState>(this as TDataContext, hooks, state);
            await hookInvoker.InvokePreHooksAsync();

            var result = await base.SaveChangesAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return result;

            await hookInvoker.InvokePostHooksAsync();

            return result;
        }

        /// <inheritdoc />
        [NotNull]
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            // Support data-layer-only operations (e.g. DbMigration, RandomDbSeeder, ..)
            if (typeof(TState) == typeof(object))
                return SaveChangesAsync(default, cancellationToken);
                
            throw new InvalidOperationException("SaveChangesAsync(CancellationToken) is no longer supported. Use SaveChangesAsync(TState, CancellationToken) instead.");
        }

        /*
        // Overriding the function "Task<int> SaveChangesAsync()" is not needed, since it calls "Task<int> SaveChangesAsync(CancellationToken cancellationToken)" anyway.
        public override Task<int> SaveChangesAsync()
        {
            // calls SaveChangesAsync(CancellationToken cancellationToken)
            return base.SaveChangesAsync();
        }
        */

        /// <inheritdoc />
        public override int SaveChanges()
        {
            // Support data-layer-only operations (e.g. DbMigration, RandomDbSeeder, ..)
            if (typeof(TState) == typeof(object))
                return AsyncHelper.RunSync(async () => await SaveChangesAsync(default, CancellationToken.None));

            throw new InvalidOperationException("SaveChanges() is no longer supported. Use SaveChangesAsync(TState, CancellationToken) instead.");
        }

        /// <summary>
        /// Static constructor.
        /// Registers an interceptor that always prepends "ARITHABORT ON" to SQL-commands. See <see cref="T:Bitbird.Core.Data.ArithAbortOnInterceptor" />.
        /// </summary>
        /// <inheritdoc />
        static HookedStateDataContext()
        {
            var type = typeof(System.Data.Entity.SqlServer.SqlProviderServices);
            if (type == null)
                throw new Exception("Do not remove, ensures static reference to System.Data.Entity.SqlServer");

            //SET ARITHABORT ON for performance increase
            //see https://docs.microsoft.com/en-us/sql/t-sql/statements/set-arithabort-transact-sql
            DbInterception.Add(new ArithAbortOnInterceptor());
        }
        #endregion

        public IQueryable<TEntity> GetNonTrackingQuery<TEntity>() where TEntity : class
        {
            return Set<TEntity>().AsNoTracking();
        }

        public IQueryable<TEntity> GetTrackingQuery<TEntity>() where TEntity : class
        {
            return Set<TEntity>();
        }
    }
}