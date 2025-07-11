﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Xml.Linq; 

namespace SqlSugar
{
    ///<summary>
    /// ** description：IDataReader Entity Builder
    /// ** author：sunkaixuan
    /// ** date：2017/4/2
    /// ** qq:610262374
    /// </summary>
    public partial class IDataReaderEntityBuilder<T>
    {
        #region Properies
        private List<string> ReaderKeys { get; set; }
        #endregion

        #region Fields
        private SqlSugarProvider Context = null;
        private IDataReaderEntityBuilder<T> DynamicBuilder;
        private IDataRecord DataRecord;
        private static readonly MethodInfo isDBNullMethod = typeof(IDataRecord).GetMethod("IsDBNull", new Type[] { typeof(int) });
        private static readonly MethodInfo getBoolean = typeof(IDataRecord).GetMethod("GetBoolean", new Type[] { typeof(int) });
        private static readonly MethodInfo getByte = typeof(IDataRecord).GetMethod("GetByte", new Type[] { typeof(int) });
        private static readonly MethodInfo getDateTime = typeof(IDataRecord).GetMethod("GetDateTime", new Type[] { typeof(int) });
        private static readonly MethodInfo getDecimal = typeof(IDataRecord).GetMethod("GetDecimal", new Type[] { typeof(int) });
        private static readonly MethodInfo getDouble = typeof(IDataRecord).GetMethod("GetDouble", new Type[] { typeof(int) });
        private static readonly MethodInfo getFloat = typeof(IDataRecord).GetMethod("GetFloat", new Type[] { typeof(int) });
        private static readonly MethodInfo getGuid = typeof(IDataRecord).GetMethod("GetGuid", new Type[] { typeof(int) });
        private static readonly MethodInfo getInt16 = typeof(IDataRecord).GetMethod("GetInt16", new Type[] { typeof(int) });
        private static readonly MethodInfo getInt32 = typeof(IDataRecord).GetMethod("GetInt32", new Type[] { typeof(int) });
        private static readonly MethodInfo getInt64 = typeof(IDataRecord).GetMethod("GetInt64", new Type[] { typeof(int) });
        private static readonly MethodInfo getString = typeof(IDataRecord).GetMethod("GetString", new Type[] { typeof(int) });
        //private static readonly MethodInfo getConvertValueMethod = typeof(IDataRecordExtensions).GetMethod("GetConvertValue");
        private static readonly MethodInfo getdatetimeoffset = typeof(IDataRecordExtensions).GetMethod("Getdatetimeoffset");
        private static readonly MethodInfo getdatetimeoffsetDate = typeof(IDataRecordExtensions).GetMethod("GetdatetimeoffsetDate");
        private static readonly MethodInfo getStringGuid = typeof(IDataRecordExtensions).GetMethod("GetStringGuid");
        private static readonly MethodInfo getXelement = typeof(IDataRecordExtensions).GetMethod("GetXelement");
        private static readonly MethodInfo getConvertStringGuid = typeof(IDataRecordExtensions).GetMethod("GetConvertStringGuid");
        private static readonly MethodInfo getEnum = typeof(IDataRecordExtensions).GetMethod("GetEnum");
        private static readonly MethodInfo getConvertString = typeof(IDataRecordExtensions).GetMethod("GetConvertString");
        private static readonly MethodInfo getConvertFloat = typeof(IDataRecordExtensions).GetMethod("GetConvertFloat");
        private static readonly MethodInfo getConvertBoolean = typeof(IDataRecordExtensions).GetMethod("GetConvertBoolean");
        private static readonly MethodInfo getConvertByte = typeof(IDataRecordExtensions).GetMethod("GetConvertByte");
        private static readonly MethodInfo getConvertChar = typeof(IDataRecordExtensions).GetMethod("GetConvertChar");
        private static readonly MethodInfo getConvertDateTime = typeof(IDataRecordExtensions).GetMethod("GetConvertDateTime");
        private static readonly MethodInfo getConvertTime = typeof(IDataRecordExtensions).GetMethod("GetConvertTime");
        private static readonly MethodInfo getTime = typeof(IDataRecordExtensions).GetMethod("GetTime");
        private static readonly MethodInfo getConvertDecimal = typeof(IDataRecordExtensions).GetMethod("GetConvertDecimal");
        private static readonly MethodInfo getConvertDouble = typeof(IDataRecordExtensions).GetMethod("GetConvertDouble");
        private static readonly MethodInfo getConvertDoubleToFloat = typeof(IDataRecordExtensions).GetMethod("GetConvertDoubleToFloat");
        private static readonly MethodInfo getConvertGuid = typeof(IDataRecordExtensions).GetMethod("GetConvertGuid");
        private static readonly MethodInfo getConvertInt16 = typeof(IDataRecordExtensions).GetMethod("GetConvertInt16");
        private static readonly MethodInfo getConvertInt32 = typeof(IDataRecordExtensions).GetMethod("GetConvertInt32");
        private static readonly MethodInfo getConvertInt64 = typeof(IDataRecordExtensions).GetMethod("GetConvetInt64");
        private static readonly MethodInfo getConvertEnum_Null = typeof(IDataRecordExtensions).GetMethod("GetConvertEnum_Null");
        private static readonly MethodInfo getConvertdatetimeoffset = typeof(IDataRecordExtensions).GetMethod("GetConvertdatetimeoffset");
        private static readonly MethodInfo getConvertdatetimeoffsetDate = typeof(IDataRecordExtensions).GetMethod("GetConvertdatetimeoffsetDate");
        private static readonly MethodInfo getOtherNull = typeof(IDataRecordExtensions).GetMethod("GetOtherNull");
        private static readonly MethodInfo getOther = typeof(IDataRecordExtensions).GetMethod("GetOther");
        private static readonly MethodInfo getJson = typeof(IDataRecordExtensions).GetMethod("GetJson");
        private static readonly MethodInfo getArray = typeof(IDataRecordExtensions).GetMethod("GetArray");
        private static readonly MethodInfo getEntity = typeof(IDataRecordExtensions).GetMethod("GetEntity", new Type[] { typeof(SqlSugarProvider) });
        private static readonly MethodInfo getMyIntNull = typeof(IDataRecordExtensions).GetMethod("GetMyIntNull");
        private static readonly MethodInfo getMyInt= typeof(IDataRecordExtensions).GetMethod("GetMyInt");

        private delegate T Load(IDataRecord dataRecord);
        private Load handler;
        #endregion

        #region Constructor
        private IDataReaderEntityBuilder()
        {

        }

        public IDataReaderEntityBuilder(SqlSugarProvider context, IDataRecord dataRecord, List<string> fieldNames)
        {
            this.Context = context;
            this.DataRecord = dataRecord;
            this.DynamicBuilder = new IDataReaderEntityBuilder<T>();
            this.ReaderKeys = fieldNames;
        }
        #endregion

        #region Public methods
        public T Build(IDataRecord dataRecord)
        {
            return handler(dataRecord);
        }

        public IDataReaderEntityBuilder<T> CreateBuilder(Type type)
        {
            DynamicMethod method = new DynamicMethod("SqlSugarEntity", type,
            new Type[] { typeof(IDataRecord) }, type, true);
            ILGenerator generator = method.GetILGenerator();
            LocalBuilder result = generator.DeclareLocal(type);
            generator.Emit(OpCodes.Newobj, type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                 null, Type.EmptyTypes, null));
            generator.Emit(OpCodes.Stloc, result);
            this.Context.InitMappingInfo(type);
            var columnInfos = this.Context.EntityMaintenance.GetEntityInfoWithAttr(type).Columns;
            foreach (var columnInfo in columnInfos)
            {
                string fileName = columnInfo.DbColumnName ?? columnInfo.PropertyName;
                if (columnInfo.IsIgnore && !this.ReaderKeys.Any(it => it.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    continue;
                }
                else if (columnInfo.ForOwnsOnePropertyInfo!=null) 
                {
                    continue;
                }
                if (columnInfo != null && columnInfo.PropertyInfo.GetSetMethod(true) != null)
                {
                    var isGemo = columnInfo.PropertyInfo?.PropertyType?.FullName=="NetTopologySuite.Geometries.Geometry";
                    if (isGemo == false && columnInfo.PropertyInfo?.PropertyType?.FullName == "Kdbndp.LegacyPostgis.PostgisGeometry") 
                    {
                        isGemo = true;
                    }
                    if (!isGemo&&columnInfo.PropertyInfo.PropertyType.IsClass() && columnInfo.PropertyInfo.PropertyType != UtilConstants.ByteArrayType && columnInfo.PropertyInfo.PropertyType != UtilConstants.ObjType)
                    {
                        if (this.ReaderKeys.Any(it => it.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            BindClass(generator, result, columnInfo, ReaderKeys.First(it => it.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)));
                        }
                        else if (this.ReaderKeys.Any(it => it.Equals(columnInfo.PropertyName, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            BindClass(generator, result, columnInfo, ReaderKeys.First(it => it.Equals(columnInfo.PropertyName, StringComparison.CurrentCultureIgnoreCase)));
                        }
                    }
                    else if (!isGemo && columnInfo.IsJson && columnInfo.PropertyInfo.PropertyType != UtilConstants.StringType)
                    {   //json is struct
                        if (this.ReaderKeys.Any(it => it.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            BindClass(generator, result, columnInfo, ReaderKeys.First(it => it.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)));
                        }
                    }
                    else
                    {
                        if (this.ReaderKeys.Any(it => it.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            BindField(generator, result, columnInfo, ReaderKeys.First(it => it.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)));
                        }
                        else if (this.ReaderKeys.Any(it => it.Equals(columnInfo.PropertyName, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            BindField(generator, result, columnInfo, ReaderKeys.First(it => it.Equals(columnInfo.PropertyName, StringComparison.CurrentCultureIgnoreCase)));
                        }
                    }
                }
            }
            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Ret);
            DynamicBuilder.handler = (Load)method.CreateDelegate(typeof(Load));
            return DynamicBuilder;
        }

        #endregion

        #region Private methods
        private void BindCustomFunc(ILGenerator generator, LocalBuilder result, EntityColumnInfo columnInfo, string fieldName) 
        {
            int i = DataRecord.GetOrdinal(fieldName);
            Label endIfLabel = generator.DefineLabel();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4, i);
            generator.Emit(OpCodes.Callvirt, isDBNullMethod);
            generator.Emit(OpCodes.Brtrue, endIfLabel);
            generator.Emit(OpCodes.Ldloc, result);
            //generator.Emit(OpCodes.Ldarg_0);
            //generator.Emit(OpCodes.Ldc_I4, i);
            var method = (columnInfo.SqlParameterDbType as Type).GetMethod("QueryConverter");
            method = method.MakeGenericMethod(new Type[] { columnInfo.PropertyInfo.PropertyType });
            Type type = (columnInfo.SqlParameterDbType as Type);
            //ConstructorInfo info = type.GetConstructor(Type.EmptyTypes);
            //il.Emit(OpCodes.Newobj, info);
            generator.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4, i);
            //method = (columnInfo.SqlParameterDbType as Type).GetMethod("QueryConverter");
            //method = method.MakeGenericMethod(new Type[] { columnInfo.PropertyInfo.PropertyType });
            if (method.IsVirtual)
                generator.Emit(OpCodes.Callvirt, method);
            else
                generator.Emit(OpCodes.Call, method);
            generator.Emit(OpCodes.Callvirt, columnInfo.PropertyInfo.GetSetMethod(true));
            generator.MarkLabel(endIfLabel);
        }
        private void BindClass(ILGenerator generator, LocalBuilder result, EntityColumnInfo columnInfo, string fieldName)
        {

            if (columnInfo.SqlParameterDbType is Type)
            {
                BindCustomFunc(generator, result, columnInfo, fieldName);
                return;
            }

            if (columnInfo.IsJson)
            {
                MethodInfo jsonMethod = getJson.MakeGenericMethod(columnInfo.PropertyInfo.PropertyType);
                int i = DataRecord.GetOrdinal(fieldName);
                Label endIfLabel = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Callvirt, isDBNullMethod);
                generator.Emit(OpCodes.Brtrue, endIfLabel);
                generator.Emit(OpCodes.Ldloc, result);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4, i);
                var insertBuilder = InstanceFactory.GetInsertBuilder(this.Context?.CurrentConnectionConfig);
                if (insertBuilder?.DeserializeObjectFunc != null)
                {
                    if (IDataRecordExtensions.DeserializeObjectFunc == null)
                    {
                        IDataRecordExtensions.DeserializeObjectFunc = insertBuilder.DeserializeObjectFunc;
                    }
                    jsonMethod =typeof(IDataRecordExtensions).GetMethod("GetDeserializeObject").MakeGenericMethod(columnInfo.PropertyInfo.PropertyType);
                }
                generator.Emit(OpCodes.Call, jsonMethod);
                generator.Emit(OpCodes.Callvirt, columnInfo.PropertyInfo.GetSetMethod(true));
                generator.MarkLabel(endIfLabel);
            }
            if (columnInfo.IsArray)
            {
                MethodInfo arrayMehtod = getArray.MakeGenericMethod(columnInfo.PropertyInfo.PropertyType);
                int i = DataRecord.GetOrdinal(fieldName);
                Label endIfLabel = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Callvirt, isDBNullMethod);
                generator.Emit(OpCodes.Brtrue, endIfLabel);
                generator.Emit(OpCodes.Ldloc, result);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Call, arrayMehtod);
                generator.Emit(OpCodes.Callvirt, columnInfo.PropertyInfo.GetSetMethod(true));
                generator.MarkLabel(endIfLabel);
            }
            else if (columnInfo.UnderType == typeof(XElement)) 
            {
                int i = DataRecord.GetOrdinal(fieldName);
                Label endIfLabel = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Callvirt, isDBNullMethod);
                generator.Emit(OpCodes.Brtrue, endIfLabel);
                generator.Emit(OpCodes.Ldloc, result);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4, i);
                BindMethod(generator, columnInfo, i);
                generator.Emit(OpCodes.Callvirt, columnInfo.PropertyInfo.GetSetMethod(true));
                generator.MarkLabel(endIfLabel);
            } 
        }
        private void BindField(ILGenerator generator, LocalBuilder result, EntityColumnInfo columnInfo, string fieldName)
        {
            if (columnInfo.SqlParameterDbType is Type)
            {
                BindCustomFunc(generator,result, columnInfo, fieldName);
                return;
            }
            int i = DataRecord.GetOrdinal(fieldName);
            Label endIfLabel = generator.DefineLabel();

            //2023-3-8
            Label tryStart = generator.BeginExceptionBlock();//begin try
            //2023-3-8 

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4, i);
            generator.Emit(OpCodes.Callvirt, isDBNullMethod);
            generator.Emit(OpCodes.Brtrue, endIfLabel);
            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4, i);
            BindMethod(generator, columnInfo, i);
            generator.Emit(OpCodes.Callvirt, columnInfo.PropertyInfo.GetSetMethod(true));
            generator.MarkLabel(endIfLabel);

            //2023-3-8
            generator.Emit(OpCodes.Leave, tryStart);//eng try
            generator.BeginCatchBlock(typeof(Exception));//begin catch
            generator.Emit(OpCodes.Ldstr, ErrorMessage.GetThrowMessage($"{columnInfo.EntityName} {columnInfo.PropertyName} bind error", $"{columnInfo.PropertyName}绑定到{columnInfo.EntityName}失败,可以试着换一个类型，或者使用ORM自定义类型实现"));//thow message
            generator.Emit(OpCodes.Newobj, typeof(Exception).GetConstructor(new Type[] { typeof(string) }));
            generator.Emit(OpCodes.Throw);
            generator.EndExceptionBlock();
            //2023-3-8
        }
        private void BindMethod(ILGenerator generator, EntityColumnInfo columnInfo, int ordinal)
        {
            IDbBind bind = Context.Ado.DbBind;
            bool isNullableType = false;
            MethodInfo method = null;
            Type bindPropertyType = UtilMethods.GetUnderType(columnInfo.PropertyInfo, ref isNullableType);
            string dbTypeName = UtilMethods.GetParenthesesValue(DataRecord.GetDataTypeName(ordinal));
            if (dbTypeName.IsNullOrEmpty())
            {
                dbTypeName = bindPropertyType.Name;
            }
            string propertyName = columnInfo.PropertyName;
            string validPropertyName = bind.GetPropertyTypeName(dbTypeName);
            validPropertyName = validPropertyName == "byte[]" ? "byteArray" : validPropertyName;
            CSharpDataType validPropertyType = (CSharpDataType)Enum.Parse(typeof(CSharpDataType), validPropertyName);

            #region NoSql
            if (this.Context.Ado is AdoProvider provider) 
            {
                if (provider.IsNoSql) 
                {
                    method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);
                    if (method.IsVirtual)
                        generator.Emit(OpCodes.Callvirt, method);
                    else
                        generator.Emit(OpCodes.Call, method);
                    return;
                }
            }
            #endregion

            #region Sqlite Logic
            if (this.Context.CurrentConnectionConfig.DbType == DbType.Sqlite)
            {
                if (bindPropertyType.IsEnum())
                {
                    method = isNullableType ? getConvertEnum_Null.MakeGenericMethod(bindPropertyType) : getEnum.MakeGenericMethod(bindPropertyType);
                }
                else if (bindPropertyType == UtilConstants.IntType)
                {
                    method = isNullableType ? getConvertInt32 : getInt32;
                }
                else if (bindPropertyType == UtilConstants.DateTimeOffsetType&&SugarCompatible.IsFramework)
                {
                    method = isNullableType ? getConvertdatetimeoffset : getdatetimeoffset;
                }
                else if (bindPropertyType == UtilConstants.ByteType)
                {
                    method = isNullableType ? getConvertByte : getByte;
                }
                else if (bindPropertyType == UtilConstants.StringType && dbTypeName?.ToLower() == "timestamp")
                {
                    method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);
                }
                else if (dbTypeName.EqualCase("STRING"))
                {
                    method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);
                }
                else if (bindPropertyType == UtilConstants.StringType&&validPropertyName == "int") 
                {
                    method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);
                }
                else if (bindPropertyType == UtilConstants.StringType)
                {
                    method = getString;
                }
                else
                {
                    method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);
                }
                if (method.IsVirtual)
                    generator.Emit(OpCodes.Callvirt, method);
                else
                    generator.Emit(OpCodes.Call, method);
                return;
            };
            #endregion

            #region Common Database Logic
            string bindProperyTypeName = bindPropertyType.Name.ToLower();
            bool isEnum = bindPropertyType.IsEnum();
            if (isEnum) { validPropertyType = CSharpDataType.@enum; }
            switch (validPropertyType)
            {
                case CSharpDataType.@int:
                    CheckType(bind.IntThrow, bindProperyTypeName, validPropertyName, propertyName);
                    if (bindProperyTypeName.IsContainsIn("int", "int32"))
                        method = isNullableType ? getConvertInt32 : getInt32;
                    if (bindProperyTypeName.IsContainsIn("int64"))
                        method = null;
                    if (bindProperyTypeName.IsContainsIn("byte"))
                        method = isNullableType ? getConvertByte : getByte;
                    if (bindProperyTypeName.IsContainsIn("int16"))
                        method = isNullableType ? getConvertInt16 : getInt16;
                    if (bindProperyTypeName == "uint32"&&this.Context.CurrentConnectionConfig.DbType.IsIn(DbType.MySql,DbType.MySqlConnector))
                        method = null;
                    if (bindPropertyType ==UtilConstants.IntType&& this.Context.CurrentConnectionConfig.DbType == DbType.OceanBaseForOracle) 
                        method = isNullableType ? getMyIntNull : getMyInt;
                    if (bindProperyTypeName == "int16")
                        method = null;
                    break;
                case CSharpDataType.@bool:
                    if (bindProperyTypeName == "bool" || bindProperyTypeName == "boolean")
                        method = isNullableType ? getConvertBoolean : getBoolean;
                    break;
                case CSharpDataType.@string:
                    if (this.Context.CurrentConnectionConfig.DbType != DbType.Oracle)
                    {
                        CheckType(bind.StringThrow, bindProperyTypeName, validPropertyName, propertyName);
                    }
                    method = getString;
                    if (bindProperyTypeName == "guid")
                    {
                        method = isNullableType ? getConvertStringGuid : getStringGuid;
                    }
                    else if (bindProperyTypeName == "xelement")
                    {
                        method = getXelement;
                    }
                    else if (dbTypeName == "CHAR" && DataRecord.GetDataTypeName(ordinal) == "CHAR(36)")
                    {
                        method = null;
                    }
                    else if (bindPropertyType.Name == "Char") 
                    {
                        method = null;
                    }
                    break;
                case CSharpDataType.DateTime:
                    CheckType(bind.DateThrow, bindProperyTypeName, validPropertyName, propertyName);
                    if (bindProperyTypeName == "datetime")
                        method = isNullableType ? getConvertDateTime : getDateTime;
                    if (bindProperyTypeName == "datetime" && dbTypeName.ToLower() == "time")
                        method = isNullableType ? getConvertTime : getTime;
                    if (bindProperyTypeName == "datetimeoffset")
                        method = isNullableType ? getConvertdatetimeoffset : getdatetimeoffset;
                    break;
                case CSharpDataType.@decimal:
                    CheckType(bind.DecimalThrow, bindProperyTypeName, validPropertyName, propertyName);
                    if (bindProperyTypeName == "decimal")
                        method = isNullableType ? getConvertDecimal : getDecimal;
                    break;
                case CSharpDataType.@float:
                case CSharpDataType.@double:
                    CheckType(bind.DoubleThrow, bindProperyTypeName, validPropertyName, propertyName);
                    if (bindProperyTypeName.IsIn("double", "single") && dbTypeName != "real")
                        method = isNullableType ? getConvertDouble : getDouble;
                    else
                        method = isNullableType ? getConvertFloat : getFloat;
                    if (dbTypeName.Equals("float", StringComparison.CurrentCultureIgnoreCase) && isNullableType && bindProperyTypeName.Equals("single", StringComparison.CurrentCultureIgnoreCase))
                    {
                        method = getConvertDoubleToFloat;
                    }
                    if (bindPropertyType == UtilConstants.DecType)
                    {
                        method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);
                    }
                    if (bindPropertyType == UtilConstants.IntType)
                    {
                        method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);
                    }
                    if (bindProperyTypeName == "string") 
                    {
                        method = null;
                    }
                    break;
                case CSharpDataType.Guid:
                    CheckType(bind.GuidThrow, bindProperyTypeName, validPropertyName, propertyName);
                    if (bindProperyTypeName == "guid")
                        method = isNullableType ? getConvertGuid : getGuid;
                    break;
                case CSharpDataType.@byte:
                    if (bindProperyTypeName == "byte")
                        method = isNullableType ? getConvertByte : getByte;
                    break;
                case CSharpDataType.@enum:
                    method = isNullableType ? getConvertEnum_Null.MakeGenericMethod(bindPropertyType) : getEnum.MakeGenericMethod(bindPropertyType);
                    break;
                case CSharpDataType.@short:
                    CheckType(bind.ShortThrow, bindProperyTypeName, validPropertyName, propertyName);
                    if (bindProperyTypeName == "int16" || bindProperyTypeName == "short")
                        method = isNullableType ? getConvertInt16 : getInt16;
                    break;
                case CSharpDataType.@long:
                    if (bindProperyTypeName == "int64" || bindProperyTypeName == "long")
                        method = isNullableType ? getConvertInt64 : getInt64;
                    break;
                case CSharpDataType.DateTimeOffset:
                    method = isNullableType ? getConvertdatetimeoffset : getdatetimeoffset;
                    if (bindProperyTypeName == "datetime")
                        method = isNullableType ? getConvertdatetimeoffsetDate : getdatetimeoffsetDate;
                    break;
                case CSharpDataType.Single:
                    break;
                default:
                    method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);
                    break;
            }
            if (method == null && bindPropertyType == UtilConstants.StringType)
            {
                method = getConvertString;
            }
            if (bindPropertyType == UtilConstants.ObjType)
            {
                method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);
            }
            if (method == null)
                method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);


            if (method.IsVirtual)
                generator.Emit(OpCodes.Callvirt, method);
            else
                generator.Emit(OpCodes.Call, method);
            #endregion
        }


        private void CheckType(List<string> invalidTypes, string bindProperyTypeName, string validPropertyType, string propertyName)
        {
            var isAny = invalidTypes.Contains(bindProperyTypeName);
            if (isAny)
            {
                throw new SqlSugarException(string.Format("{0} can't  convert {1} to {2}", propertyName, validPropertyType, bindProperyTypeName));
            }
        }
        #endregion
    } 
}
