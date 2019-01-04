using System;
using System.Linq;

namespace Bitbird.Core.Data.Net
{
    public static class TokenExtensions
    {
        #region Filters
        // Currently supported are key=long versions of tokens. TODO: use generic approach. 
        public static IQueryable<T> FilterToken<T>(this IQueryable<T> query, TokenValidity tokenValidity)
            where T : class, IToken<long>
        {
            return query.FilterTokenInternally<T, long>(tokenValidity, null, null);
        }
        public static IQueryable<T> FilterToken<T>(this IQueryable<T> query, long tokenId, TokenValidity tokenValidity)
            where T : class, IToken<long>
        {
            return query.FilterTokenInternally<T, long>(tokenValidity, tokenId, null);
        }
        public static IQueryable<T> FilterToken<T>(this IQueryable<T> query, string tokenKey, TokenValidity tokenValidity)
            where T : class, IToken<long>
        {
            return query.FilterTokenInternally<T, long>(tokenValidity, null, tokenKey);
        }

        public static IQueryable<T> FilterToken<T>(this IQueryable<T> query, string tokenKey, long userId, TokenValidity tokenValidity) 
            where T : class, IUserToken<long, long>
        {
            return query.FilterTokenInternally<T, long>(tokenValidity, null, tokenKey)
                .Where(_ => _.UserId.Equals(userId));
        }

        private static IQueryable<T> FilterTokenInternally<T, TTokenKey>(this IQueryable<T> query, TokenValidity tokenValidity, TTokenKey? tokenId, string tokenKey) 
            where T : class, IToken<TTokenKey>
            where TTokenKey : struct, IEquatable<TTokenKey>
        {
            switch (tokenValidity)
            {
                case TokenValidity.Valid:
                    query = query.Where(_ => _.ValidUntil >= DateTime.UtcNow);
                    break;
                case TokenValidity.NotValid:
                    query = query.Where(_ => _.ValidUntil < DateTime.UtcNow);
                    break;
                case TokenValidity.Any:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tokenValidity));
            }

            if (tokenId.HasValue)
                query = query.Where(_ => _.Id.Equals(tokenId.Value));

            if (tokenKey != null)
                query = query.Where(_ => _.TokenKey == tokenKey);

            return query;
        }
        #endregion
    }
}