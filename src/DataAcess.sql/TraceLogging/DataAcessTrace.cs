using System;
using RepoDb;
using RepoDb.Interfaces;

namespace DataManager.DataAccess.TraceLogging
{
    public class DataAcessTrace 
    {
        //public void AfterAverage(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterAverageAll(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterBatchQuery(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterCount(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterCountAll(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterDelete(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterDeleteAll(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterExecuteNonQuery(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterExecuteQuery(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterExecuteReader(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterExecuteScalar(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterExists(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterInsert(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterInsertAll(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterMax(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterMaxAll(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterMerge(TraceLog Log)
        //{
        //    Console.WriteLine($"AfterMerge: {Log.Statement}");
        //    Console.WriteLine($"AfterMerge: {Log.ExecutionTime.TotalSeconds}");
        //    Console.WriteLine($"AfterMerge: {Log.SessionId}");
        //    Console.WriteLine($"AfterMerge: {Log.Parameter}");
        //    Console.WriteLine($"AfterMerge: {Log.Result}");
        //}

        //public void AfterMergeAll(TraceLog Log)
        //{
        //    Console.WriteLine($"AfterMergeAll: {Log.Statement}");
        //    Console.WriteLine($"AfterMergeAll: {Log.ExecutionTime.TotalSeconds}");
        //    Console.WriteLine($"AfterMergeAll: {Log.SessionId}");
        //    Console.WriteLine($"AfterMergeAll: {Log.Parameter}");
        //    Console.WriteLine($"AfterMergeAll: {Log.Result}");
        //}

        //public void AfterMin(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterMinAll(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterQuery(TraceLog Log)
        //{
        //    Console.WriteLine($"After UpdateSingle: {Log.Statement}");
        //    Console.WriteLine($"After UpdateSingle: {Log.ExecutionTime.TotalSeconds}");
        //    Console.WriteLine($"After UpdateSingle: {Log.SessionId}");
        //    Console.WriteLine($"After UpdateSingle: {Log.Parameter}");
        //    Console.WriteLine($"After UpdateSingle: {Log.Result}");
        //}

        //public void AfterQueryAll(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterQueryMultiple(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterSum(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterSumAll(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterTruncate(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void AfterUpdate(TraceLog Log)
        //{
        //    Console.WriteLine($"After UpdateSingle: {Log.Statement}");
        //    Console.WriteLine($"After UpdateSingle: {Log.ExecutionTime.TotalSeconds}");
        //    Console.WriteLine($"After UpdateSingle: {Log.SessionId}");
        //    Console.WriteLine($"After UpdateSingle: {Log.Parameter}");
        //    Console.WriteLine($"After UpdateSingle: {Log.Result}");
        //}

        //public void AfterUpdateAll(TraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeAverage(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeAverageAll(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeBatchQuery(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeCount(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeCountAll(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeDelete(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeDeleteAll(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeExecuteNonQuery(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeExecuteQuery(CancellableTraceLog Log)
        //{
        //    Console.WriteLine($"After UpdateSingle: {Log.Statement}");
        //    Console.WriteLine($"After UpdateSingle: {Log.ExecutionTime.TotalSeconds}");
        //    Console.WriteLine($"After UpdateSingle: {Log.SessionId}");
        //    Console.WriteLine($"After UpdateSingle: {Log.Parameter}");
        //    Console.WriteLine($"After UpdateSingle: {Log.Result}");
        //}

        //public void BeforeExecuteReader(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeExecuteScalar(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeExists(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeInsert(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeInsertAll(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeMax(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeMaxAll(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeMerge(CancellableTraceLog Log)
        //{
        //    Console.WriteLine($"BeforeMerge: {Log.Statement}");
        //    Console.WriteLine($"BeforeMerge: {Log.ExecutionTime.TotalSeconds}");
        //    Console.WriteLine($"BeforeMerge: {Log.SessionId}");
        //    Console.WriteLine($"BeforeMerge: {Log.Parameter}");
        //    Console.WriteLine($"BeforeMerge: {Log.Result}");
        //}

        //public void BeforeMergeAll(CancellableTraceLog Log)
        //{
        //    Console.WriteLine($"BeforeMergeAll: {Log.Statement}");
        //    Console.WriteLine($"BeforeMergeAll: {Log.ExecutionTime.TotalSeconds}");
        //    Console.WriteLine($"BeforeMergeAll: {Log.SessionId}");
        //    Console.WriteLine($"BeforeMergeAll: {Log.Parameter}");
        //    Console.WriteLine($"BeforeMergeAll: {Log.Result}");
        //}

        //public void BeforeMin(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeMinAll(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeQuery(CancellableTraceLog Log)
        //{
        //    Console.WriteLine($"BeforeQuery: {Log.Statement}");
        //    Console.WriteLine($"BeforeQuery: {Log.ExecutionTime.TotalSeconds}");
        //    Console.WriteLine($"BeforeQuery: {Log.SessionId}");
        //    Console.WriteLine($"BeforeQuery: {Log.Parameter}");
        //    Console.WriteLine($"BeforeQuery: {Log.Result}");
        //}

        //public void BeforeQueryAll(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeQueryMultiple(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeSum(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeSumAll(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeTruncate(CancellableTraceLog Log)
        //{
        //    throw new NotImplementedException();
        //}

        //public void BeforeUpdate(CancellableTraceLog Log)
        //{
        //    Console.WriteLine($"Before UpdateSingle: {Log.Statement}");
        //    Console.WriteLine($"Before UpdateSingle: {Log.ExecutionTime.TotalSeconds}");
        //    Console.WriteLine($"Before UpdateSingle: {Log.SessionId}");
        //    Console.WriteLine($"Before UpdateSingle: {Log.Parameter}");
        //    Console.WriteLine($"Before UpdateSingle: {Log.Result}");
        //}

        //public void BeforeUpdateAll(CancellableTraceLog Log)
        //{
        //    Console.WriteLine($"Before UpdateAll: {Log.Statement}");
        //    Console.WriteLine($"Before UpdateAll: {Log.ExecutionTime.TotalSeconds}");
        //    Console.WriteLine($"Before UpdateAll: {Log.SessionId}");
        //    Console.WriteLine($"Before UpdateAll: {Log.Parameter}");
        //    Console.WriteLine($"Before UpdateAll: {Log.Result}");
        //}
    }
}
