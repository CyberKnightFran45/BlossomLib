using System;

/// <summary> A delegate that defines a transformation function for processing byte data. </summary>

public delegate NativeMemoryOwner<byte> BytesTransform(ReadOnlySpan<byte> input);

/// <summary> A delegate that defines a transformation function for processing byte data
/// with an extra argument. </summary>

public delegate NativeMemoryOwner<byte> BytesTransform2(ReadOnlySpan<byte> input,
                                                        ReadOnlySpan<byte> arg1);