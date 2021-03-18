using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct State_Selected : IComponentData{}
[Serializable]
public struct State_Unassigned : IComponentData { }
[Serializable]
public struct State_Hovered : IComponentData { }
