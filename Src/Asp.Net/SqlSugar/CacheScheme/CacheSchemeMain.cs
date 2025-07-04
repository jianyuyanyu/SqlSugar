﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSugar
{
    internal class CacheSchemeMain
    {
        public static T GetOrCreate<T>(ICacheService cacheService, QueryBuilder queryBuilder, Func<T> getData, int cacheDurationInSeconds, SqlSugarProvider context,string cacheKey)
        {
            CacheKey key = CacheKeyBuider.GetKey(context, queryBuilder);
            key.AppendKey = cacheKey;
            string keyString = key.ToString();
            var result = cacheService.GetOrCreate(keyString, getData, cacheDurationInSeconds);
            return result;
        }

        public static void RemoveCache(ICacheService cacheService, string tableName)
        {
            if (cacheService == null)
            {
                return;
            }
            if (StaticConfig.CacheRemoveByLikeStringFunc != null)
            {
                StaticConfig.CacheRemoveByLikeStringFunc(cacheService, UtilConstants.Dot + tableName.ToLower() + UtilConstants.Dot);
                return;
            }
            var keys = cacheService.GetAllKey<string>();
            if (keys.HasValue())
            {
                foreach (var item in keys)
                {
                    if (item.ToLower().Contains(UtilConstants.Dot + tableName.ToLower() + UtilConstants.Dot))
                    {
                        cacheService.Remove<string>(item);
                    }
                }
            }
        }
        public static void RemoveCacheByLike(ICacheService cacheService, string likeString)
        {
            if (cacheService == null)
            {
                return;
            }
            if (StaticConfig.CacheRemoveByLikeStringFunc != null) 
            {
                StaticConfig.CacheRemoveByLikeStringFunc(cacheService, likeString);
                return;
            }
            var keys = cacheService.GetAllKey<string>();
            if (keys.HasValue())
            {
                foreach (var item in keys)
                {
                    if (item!=null&&item.ToLower().Contains(likeString.ToLower()))
                    {
                        cacheService.Remove<string>(item);
                    }
                }
            }
        }
    }
}
