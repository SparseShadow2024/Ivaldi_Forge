// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace Ivaldi.FlatBuffers.AnimatorData
{

using global::System;
using global::System.Collections.Generic;
using global::Google.FlatBuffers;

public struct AnimatorDataTable : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_25_2_10(); }
  public static AnimatorDataTable GetRootAsAnimatorDataTable(ByteBuffer _bb) { return GetRootAsAnimatorDataTable(_bb, new AnimatorDataTable()); }
  public static AnimatorDataTable GetRootAsAnimatorDataTable(ByteBuffer _bb, AnimatorDataTable obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public static bool VerifyAnimatorDataTable(ByteBuffer _bb) {Google.FlatBuffers.Verifier verifier = new Google.FlatBuffers.Verifier(_bb); return verifier.VerifyBuffer("", false, AnimatorDataTableVerify.Verify); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public AnimatorDataTable __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public Ivaldi.FlatBuffers.AnimatorData.AnimatorData? DataList(int j) { int o = __p.__offset(4); return o != 0 ? (Ivaldi.FlatBuffers.AnimatorData.AnimatorData?)(new Ivaldi.FlatBuffers.AnimatorData.AnimatorData()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
  public int DataListLength { get { int o = __p.__offset(4); return o != 0 ? __p.__vector_len(o) : 0; } }

  public static Offset<Ivaldi.FlatBuffers.AnimatorData.AnimatorDataTable> CreateAnimatorDataTable(FlatBufferBuilder builder,
      VectorOffset DataListOffset = default(VectorOffset)) {
    builder.StartTable(1);
    AnimatorDataTable.AddDataList(builder, DataListOffset);
    return AnimatorDataTable.EndAnimatorDataTable(builder);
  }

  public static void StartAnimatorDataTable(FlatBufferBuilder builder) { builder.StartTable(1); }
  public static void AddDataList(FlatBufferBuilder builder, VectorOffset dataListOffset) { builder.AddOffset(0, dataListOffset.Value, 0); }
  public static VectorOffset CreateDataListVector(FlatBufferBuilder builder, Offset<Ivaldi.FlatBuffers.AnimatorData.AnimatorData>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static VectorOffset CreateDataListVectorBlock(FlatBufferBuilder builder, Offset<Ivaldi.FlatBuffers.AnimatorData.AnimatorData>[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateDataListVectorBlock(FlatBufferBuilder builder, ArraySegment<Offset<Ivaldi.FlatBuffers.AnimatorData.AnimatorData>> data) { builder.StartVector(4, data.Count, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateDataListVectorBlock(FlatBufferBuilder builder, IntPtr dataPtr, int sizeInBytes) { builder.StartVector(1, sizeInBytes, 1); builder.Add<Offset<Ivaldi.FlatBuffers.AnimatorData.AnimatorData>>(dataPtr, sizeInBytes); return builder.EndVector(); }
  public static void StartDataListVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static Offset<Ivaldi.FlatBuffers.AnimatorData.AnimatorDataTable> EndAnimatorDataTable(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<Ivaldi.FlatBuffers.AnimatorData.AnimatorDataTable>(o);
  }
  public static void FinishAnimatorDataTableBuffer(FlatBufferBuilder builder, Offset<Ivaldi.FlatBuffers.AnimatorData.AnimatorDataTable> offset) { builder.Finish(offset.Value); }
  public static void FinishSizePrefixedAnimatorDataTableBuffer(FlatBufferBuilder builder, Offset<Ivaldi.FlatBuffers.AnimatorData.AnimatorDataTable> offset) { builder.FinishSizePrefixed(offset.Value); }
}


static public class AnimatorDataTableVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyVectorOfTables(tablePos, 4 /*DataList*/, Ivaldi.FlatBuffers.AnimatorData.AnimatorDataVerify.Verify, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}

}
