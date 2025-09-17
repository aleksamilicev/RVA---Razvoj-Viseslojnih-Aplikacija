using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using RVA.Shared.Interfaces;
using RVA.Shared.Exceptions;

namespace RVA.Server.Data
{
    /// <summary>
    /// Bazni repository sa osnovnom implementacijom
    /// </summary>
    /// <typeparam name="T">Tip entiteta</typeparam>
    public abstract class BaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly List<T> _entities;
        protected readonly IDataStorage _dataStorage;
        protected readonly ILogger _logger;
        protected readonly string _filePath;
        protected int _nextId = 1;

        protected BaseRepository(IDataStorage dataStorage, ILogger logger, string fileName)
        {
            _entities = new List<T>();
            _dataStorage = dataStorage ?? throw new ArgumentNullException(nameof(dataStorage));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _filePath = @"C:\Users\KORISNIK\Desktop\RVA---Razvoj-Viseslojnih-Aplikacija\RVA-Project\RVA.Server\DataFiles\" +
            $"{fileName}.{_dataStorage.FileExtension}";


            LoadData();
        }

        public virtual IEnumerable<T> GetAll()
        {
            try
            {
                _logger.Debug($"Getting all {typeof(T).Name} entities");
                return _entities.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting all {typeof(T).Name} entities", ex);
                throw new RepositoryException($"Error retrieving {typeof(T).Name} entities", ex);
            }
        }

        public virtual T GetById(int id)
        {
            try
            {
                _logger.Debug($"Getting {typeof(T).Name} entity by id: {id}");
                var entity = _entities.FirstOrDefault(e => GetEntityId(e) == id);
                if (entity == null)
                {
                    _logger.Warn($"{typeof(T).Name} entity with id {id} not found");
                }
                return entity;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting {typeof(T).Name} entity by id {id}", ex);
                throw new RepositoryException($"Error retrieving {typeof(T).Name} entity", ex);
            }
        }

        public virtual IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.Debug($"Finding {typeof(T).Name} entities with predicate");
                var compiledPredicate = predicate.Compile();
                return _entities.Where(compiledPredicate).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error finding {typeof(T).Name} entities", ex);
                throw new RepositoryException($"Error finding {typeof(T).Name} entities", ex);
            }
        }

        public virtual void Add(T entity)
        {
            try
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));

                SetEntityId(entity, _nextId++);
                _entities.Add(entity);
                _logger.Info($"Added {typeof(T).Name} entity with id {GetEntityId(entity)}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error adding {typeof(T).Name} entity", ex);
                throw new RepositoryException($"Error adding {typeof(T).Name} entity", ex);
            }
        }

        public virtual void Update(T entity)
        {
            try
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));

                var id = GetEntityId(entity);
                var existingIndex = _entities.FindIndex(e => GetEntityId(e) == id);

                if (existingIndex >= 0)
                {
                    _entities[existingIndex] = entity;
                    _logger.Info($"Updated {typeof(T).Name} entity with id {id}");
                }
                else
                {
                    throw new RepositoryException($"{typeof(T).Name} entity with id {id} not found for update");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error updating {typeof(T).Name} entity", ex);
                throw;
            }
        }

        public virtual void Delete(int id)
        {
            try
            {
                var entity = GetById(id);
                if (entity != null)
                {
                    Delete(entity);
                }
                else
                {
                    throw new RepositoryException($"{typeof(T).Name} entity with id {id} not found for deletion");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error deleting {typeof(T).Name} entity with id {id}", ex);
                throw;
            }
        }

        public virtual void Delete(T entity)
        {
            try
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));

                var removed = _entities.Remove(entity);
                if (removed)
                {
                    _logger.Info($"Deleted {typeof(T).Name} entity with id {GetEntityId(entity)}");
                }
                else
                {
                    throw new RepositoryException($"{typeof(T).Name} entity not found for deletion");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error deleting {typeof(T).Name} entity", ex);
                throw;
            }
        }

        public virtual void AddRange(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null) throw new ArgumentNullException(nameof(entities));

                foreach (var entity in entities)
                {
                    SetEntityId(entity, _nextId++);
                    _entities.Add(entity);
                }
                _logger.Info($"Added range of {entities.Count()} {typeof(T).Name} entities");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error adding range of {typeof(T).Name} entities", ex);
                throw new RepositoryException($"Error adding range of {typeof(T).Name} entities", ex);
            }
        }

        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null) throw new ArgumentNullException(nameof(entities));

                foreach (var entity in entities.ToList())
                {
                    _entities.Remove(entity);
                }
                _logger.Info($"Removed range of {entities.Count()} {typeof(T).Name} entities");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error removing range of {typeof(T).Name} entities", ex);
                throw new RepositoryException($"Error removing range of {typeof(T).Name} entities", ex);
            }
        }

        public virtual void SaveChanges()
        {
            try
            {
                _logger.Debug($"Saving {typeof(T).Name} entities to storage");
                _dataStorage.SaveData(_entities, _filePath);
                _logger.Info($"Saved {_entities.Count} {typeof(T).Name} entities");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error saving {typeof(T).Name} entities", ex);
                throw new RepositoryException($"Error saving {typeof(T).Name} entities", ex);
            }
        }

        public virtual int Count()
        {
            try
            {
                return _entities.Count;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting count of {typeof(T).Name} entities", ex);
                throw new RepositoryException($"Error getting count of {typeof(T).Name} entities", ex);
            }
        }

        public virtual bool Exists(int id)
        {
            try
            {
                return _entities.Any(e => GetEntityId(e) == id);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error checking existence of {typeof(T).Name} entity with id {id}", ex);
                throw new RepositoryException($"Error checking existence of {typeof(T).Name} entity", ex);
            }
        }

        protected virtual void LoadData()
        {
            try
            {
                if (_dataStorage.FileExists(_filePath))
                {
                    var loadedEntities = _dataStorage.LoadData<T>(_filePath);
                    _entities.Clear();
                    _entities.AddRange(loadedEntities);

                    // Postavljanje sledeceg ID-ja
                    if (_entities.Any())
                    {
                        _nextId = _entities.Max(e => GetEntityId(e)) + 1;
                    }

                    _logger.Info($"Loaded {_entities.Count} {typeof(T).Name} entities from storage");
                }
                else
                {
                    _logger.Info($"No existing data file found for {typeof(T).Name}, starting with empty collection");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading {typeof(T).Name} entities from storage", ex);
                throw new RepositoryException($"Error loading {typeof(T).Name} entities", ex);
            }
        }

        protected abstract int GetEntityId(T entity);
        protected abstract void SetEntityId(T entity, int id);
    }
}