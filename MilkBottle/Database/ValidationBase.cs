using System;
using System.Collections.Generic;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;
using MilkBottle.Types;

namespace MilkBottle.Database {
    class ValidationBase<T> where T : EntityBase {
        protected Either<Exception, T> ValidateEntity( T entity ) {
            if( entity == null ) {
                return new ValidationException( "Entity cannot be null" );
            }

            return entity;
        }

        protected Either<Exception, ObjectId> ValidateObjectId( ObjectId id ) {
            if( id == null ) {
                return new ValidationException( "Object id is null" );
            }

            if( id.Equals( new ObjectId())) {
                return new ValidationException( "ObjectId is not initialized" );
            }

            return id;
        }

        protected Either<Exception, Action<IEnumerable<T>>> ValidateAction( Action<IEnumerable<T>> action ) {
            if( action == null ) {
                return new ValidationException( "Action cannot be null" );
            }

            return action;
        }

        protected Either<Exception, Action<ILiteQueryable<T>>> ValidateAction( Action<ILiteQueryable<T>> action ) {
            if( action == null ) {
                return new ValidationException( "Action cannot be null" );
            }

            return action;
        }

        protected Either<Exception, Action<ILiteCollection<T>>> ValidateAction( Action<ILiteCollection<T>> action ) {
            if( action == null ) {
                return new ValidationException( "Action cannot be null" );
            }

            return action;
        }

        protected Either<Exception, string> ValidateString( string value ) {
            if( String.IsNullOrWhiteSpace( value )) {
                return new ValidationException( "String value cannot be empty or null" );
            }

            return value;
        }
    }
}
