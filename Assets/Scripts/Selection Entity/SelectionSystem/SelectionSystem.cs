using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;
using Unity.Physics.Systems;
using Unity.Physics;
//Make sur it's always im use
//[UpdateInGroup(typeof(InitializationSystemGroup))]
[AlwaysUpdateSystem]
public class SelectionSystem : SystemBase
{
    public float3 startPosition;
    public float3 endPosition;

    public bool dragSelect;
    private float widthBoxSelect;
    private float heightBoxSelect;

    private EntityManager _entityManager;
    Entity RegUnit;

    BeginInitializationEntityCommandBufferSystem BeginInit_ECB;
    EndInitializationEntityCommandBufferSystem EndInit_ECB;

    #region RAYCAST ECS

    //==========================================================================================================================
    /// <summary>
    /// ECS RAYCAST BASIC Construction
    /// </summary>
    /// <param name="fromPosition"></param>
    /// <param name="toPosition"></param>
    /// <returns></returns>
    private Entity Raycast(float3 fromPosition, float3 toPosition)
    {
        BuildPhysicsWorld buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>(); //Seems to connect physics to the current world
        CollisionWorld collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;//Seems to connect methods uses for collision to the physics world we created

        RaycastInput raycastInput = new RaycastInput
        {
            Start = fromPosition,
            End = toPosition,
            //Layer filter
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u, //belongs to all layers
                CollidesWith = ~0u, //collides with all layers
                GroupIndex = 0,
            }
        };
        //throw a raycast
        Unity.Physics.RaycastHit raycastHit = new Unity.Physics.RaycastHit();
        if (collisionWorld.CastRay(raycastInput, out raycastHit))
        {
            //Return the entity hit
            Entity hitEntity = buildPhysicsWorld.PhysicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
            return hitEntity;
        }
        else
        {
            return Entity.Null;
        }

    }
    
    //==========================================================================================================================
    #endregion RAYCAST ECS
    // Start is called before the first frame update
    protected override void OnCreate()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        BeginInit_ECB = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        EndInit_ECB = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
    }

    protected override void OnStartRunning()
    {
        dragSelect = false;
        RegUnit = Entity.Null;
        SelectionCanvasMono.instance.selectionBox.gameObject.SetActive(false);
        widthBoxSelect = 0f;
        heightBoxSelect = 0f;
    }

    // Update is called once per frame
    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = Input.mousePosition;
            SelectionCanvasMono.instance.selectionBox.gameObject.SetActive(true); //SelectionBox SHOW
        }

        if (Input.GetMouseButton(0))
        {
            dragSelect = math.length(startPosition - (float3)Input.mousePosition) > 10 ? true : false;
            
            widthBoxSelect = Input.mousePosition.x - startPosition.x;
            heightBoxSelect = Input.mousePosition.y - startPosition.y;

            SelectionCanvasMono.instance.selectionBox.sizeDelta = new float2(math.abs(widthBoxSelect), math.abs(heightBoxSelect));
            SelectionCanvasMono.instance.selectionBox.anchoredPosition = new float2(startPosition.x, startPosition.y) + new float2(widthBoxSelect / 2, heightBoxSelect / 2);
        }

        if (Input.GetMouseButtonUp(0))
        {
            SelectionCanvasMono.instance.selectionBox.gameObject.SetActive(false); //SelectionBox HIDE
            RegUnit = Entity.Null;
            EntityCommandBuffer.ParallelWriter BeginInitecb = BeginInit_ECB.CreateCommandBuffer().AsParallelWriter(); // done at the begining
            EntityCommandBuffer.ParallelWriter EndInitecb = EndInit_ECB.CreateCommandBuffer().AsParallelWriter(); // done at the begining
            //Simple Click without drag
            if (dragSelect == false)
            {
                UnityEngine.Ray ray = Camera.main.ScreenPointToRay(startPosition);
                Entity _UnitHit = Raycast(ray.origin, ray.direction * 50000f);
                if (_entityManager.HasComponent<UnitTag>(_UnitHit))
                {
                //WE HIT SOMETHING
                    if(!Input.GetKey(KeyCode.LeftShift))
                    {
                        //DeselectALL(BeginInitecb);
                        Entities
                            .WithAny<SelectedUnitTag, RegimentSelectedTag>()
                            .WithBurst()
                            .ForEach((Entity selected, int entityInQueryIndex) =>
                            {
                                if (HasComponent<RegimentSelectedTag>(selected))
                                    BeginInitecb.RemoveComponent<RegimentSelectedTag>(entityInQueryIndex, selected);
                                else
                                    BeginInitecb.RemoveComponent<SelectedUnitTag>(entityInQueryIndex, selected);
                            }).ScheduleParallel();
                        BeginInit_ECB.AddJobHandleForProducer(Dependency);
                    }
                    _entityManager.AddComponent<SelectedUnitTag>(_UnitHit);
                    _entityManager.AddComponent<UnitNeedHighlightTag>(_UnitHit);
                    RegUnit = _UnitHit != Entity.Null ? _entityManager.GetComponentData<Parent>(_UnitHit).Value : Entity.Null; //Find the Parent/Regiment Entity of the Unit

                    _entityManager.AddComponent<RegimentUnitSelectedTag>(RegUnit);
                    Entities
                    .WithBurst()
                    .WithAll<RegimentTag, RegimentUnitSelectedTag>()
                    .WithNone<RegimentSelectedTag>()
                    .ForEach((Entity regimentSelected, int entityInQueryIndex, in DynamicBuffer<Child> unitChild) =>
                    {
                        for (int i = 0; i < unitChild.Length; i++)
                        {
                            if (!HasComponent<SelectedUnitTag>(unitChild[i].Value))
                            {
                                BeginInitecb.AddComponent<SelectedUnitTag>(entityInQueryIndex, unitChild[i].Value);
                                BeginInitecb.AddComponent<UnitNeedHighlightTag>(entityInQueryIndex, unitChild[i].Value);
                                UnityEngine.Debug.Log("REGIMENTUNIT PASS");
                            }
                        }
                        BeginInitecb.AddComponent<RegimentSelectedTag>(entityInQueryIndex, regimentSelected);
                        BeginInitecb.RemoveComponent<RegimentUnitSelectedTag>(entityInQueryIndex, regimentSelected);
                    }).Schedule();
                    BeginInit_ECB.AddJobHandleForProducer(this.Dependency);
                }
                else
                {
                    // NO TARGET
                    //DeselectALL(BeginInitecb);
                    //REMOVE SELECT COMPONENT
                    Entities
                            .WithAny<SelectedUnitTag, RegimentSelectedTag>()
                            .WithBurst()
                            .ForEach((Entity selected, int entityInQueryIndex) =>
                            {
                                if (HasComponent<RegimentSelectedTag>(selected))
                                {
                                    BeginInitecb.RemoveComponent<RegimentSelectedTag>(entityInQueryIndex, selected);
                                }
                                else
                                {
                                    BeginInitecb.RemoveComponent<SelectedUnitTag>(entityInQueryIndex, selected);
                                    BeginInitecb.AddComponent<UnitNoNeedHighlightTag>(entityInQueryIndex, selected);
                                }
                            }).ScheduleParallel();
                    BeginInit_ECB.AddJobHandleForProducer(this.Dependency);
                    //REMOVE HIGHLIGHT
                    Entities
                        .WithAll<UnitTag, UnitNoNeedHighlightTag>()
                        .WithNone<SelectedUnitTag>()
                        .WithBurst()
                        //.WithEntityQueryOptions(EntityQueryOptions.IncludeDisabled)
                        .ForEach((Entity UnitSelected, int entityInQueryIndex, in DynamicBuffer<LinkedEntityGroup> linkedEntity) =>
                        {
                            UnityEngine.Debug.Log("DPass1");
                            for (int i = 1; i < linkedEntity.Length; i++)
                            {
                                UnityEngine.Debug.Log("DPass2");
                                if (HasComponent<HighlightTag>(linkedEntity[i].Value))
                                {
                                    UnityEngine.Debug.Log("HighlightDnable Pass3");
                                    BeginInitecb.AddComponent<Disabled>(entityInQueryIndex, linkedEntity[i].Value);
                                }
                            }
                            BeginInitecb.RemoveComponent<UnitNoNeedHighlightTag>(entityInQueryIndex, UnitSelected);
                        }).Schedule();
                    BeginInit_ECB.AddJobHandleForProducer(this.Dependency);
                }
                //BeginInit_ECB.AddJobHandleForProducer(Dependency);
            }
            else
            {
                endPosition = Input.mousePosition;
                float3 lowerLeftPosition = new float3(math.min(startPosition.x, endPosition.x), math.min(startPosition.y, endPosition.y), 0);
                float3 upperRightPosition = new float3(math.max(startPosition.x, endPosition.x), math.max(startPosition.y, endPosition.y), 0);
                Entities
                    .WithStructuralChanges() // allow to use MainThread structural change , CAREFULL this does not allow BURST COMPILE
                    .WithAll<UnitTag>()//Select Only Entities wit at least this component
                    .ForEach((Entity entity, in Translation translation) =>
                    {

                        float3 entityPosition = translation.Value;
                        float3 screenPos = Camera.main.WorldToScreenPoint(entityPosition);
                        if ( (screenPos.x >= lowerLeftPosition.x) && (screenPos.y >= lowerLeftPosition.y) && (screenPos.x <= upperRightPosition.x) && (screenPos.y <= upperRightPosition.y) )
                        {
                            _entityManager.AddComponent<SelectedUnitTag>(entity); // Add SelectionComponent : ATTENTION: NEED ".WithStructuralChanges()" to work
                            _entityManager.AddComponent<UnitNeedHighlightTag>(entity);
                            Debug.Log(entity);
                        }

                    })
                    .WithoutBurst()
                    .Run();
                //.ScheduleParallel(); ATTENTION pas de burst ici , car Camera.main n'est pas une fonction ECS
                // Tout variable ou methods NON-ECS bloque burst?
            }

            if (RegUnit != Entity.Null)
            {
                /*
                _entityManager.AddComponent<RegimentUnitSelectedTag>(RegUnit);
                //SelectWholeRegiment(BeginInitecb);
                Entities
                    .WithBurst()
                    .WithAll<RegimentTag, RegimentUnitSelectedTag>()
                    .WithNone<RegimentSelectedTag>()
                    .ForEach((Entity regimentSelected, int entityInQueryIndex, in DynamicBuffer<Child> unitChild) =>
                    {
                        for (int i = 0; i < unitChild.Length; i++)
                        {
                            if (!HasComponent<SelectedUnitTag>(unitChild[i].Value))
                            {
                                BeginInitecb.AddComponent<SelectedUnitTag>(entityInQueryIndex, unitChild[i].Value);
                                BeginInitecb.AddComponent<UnitNeedHighlightTag>(entityInQueryIndex, unitChild[i].Value);
                                UnityEngine.Debug.Log("REGIMENTUNIT PASS");
                            }
                        }
                        BeginInitecb.AddComponent<RegimentSelectedTag>(entityInQueryIndex, regimentSelected);
                        BeginInitecb.RemoveComponent<RegimentUnitSelectedTag>(entityInQueryIndex, regimentSelected);
                    }).ScheduleParallel();
                BeginInit_ECB.AddJobHandleForProducer(this.Dependency);
                */
                Entities
                    .WithAll<SelectedUnitTag, UnitNeedHighlightTag>()
                    .WithBurst()
                    .ForEach((Entity UnitSelected, int entityInQueryIndex, in DynamicBuffer<Child> linkedEntity) =>
                    {
                        UnityEngine.Debug.Log("Pass1");
                        for (int i = 0; i < linkedEntity.Length; i++)
                        {
                            UnityEngine.Debug.Log("Pass2");
                            if (HasComponent<HighlightTag>(linkedEntity[i].Value))
                            {
                                UnityEngine.Debug.Log("HighlightEnable Pass3");
                                BeginInitecb.RemoveComponent<Disabled>(entityInQueryIndex, linkedEntity[i].Value);
                            }
                            else
                            {
                                UnityEngine.Debug.Log("Something goes wrong");
                            }
                        }
                        BeginInitecb.RemoveComponent<UnitNeedHighlightTag>(entityInQueryIndex, UnitSelected);
                    }).Schedule();
                EndInit_ECB.AddJobHandleForProducer(this.Dependency);
            }
            BeginInit_ECB.AddJobHandleForProducer(this.Dependency);
            EndInit_ECB.AddJobHandleForProducer(this.Dependency);
        }

    }

    /// <summary>
    /// Deselect All Unit and Regiment
    /// </summary>
    /// <param name="BeginInitecb"></param>
    #region Deselect ALL
    public void DeselectALL(EntityCommandBuffer.ParallelWriter ecb)
    {
        Entities
            .WithAny<SelectedUnitTag, RegimentSelectedTag>()
            .WithBurst()
            .ForEach((Entity selected, int entityInQueryIndex) =>
            {
                if (HasComponent<RegimentSelectedTag>(selected))
                    ecb.RemoveComponent<RegimentSelectedTag>(entityInQueryIndex, selected);
                else
                    ecb.RemoveComponent<SelectedUnitTag>(entityInQueryIndex, selected);
            }).ScheduleParallel();
    }
    #endregion Deselect ALL

    /// <summary>
    /// Select the whole regiment by:
    /// 1) retrieve Regiment with the "RegimentUnitSelectedTag" meaning that only part of the regiment is currently selected
    /// 2) Go throught the dynamicBuffer<Child> (array of entity listing all Unit composing the Regiment)
    /// 3) add component SelectedUnit to all
    /// </summary>
    /// <param name="BeginInitecb"></param>
    #region Select whole regiment
    public void SelectWholeRegiment(EntityCommandBuffer.ParallelWriter ecb)
    {
        Entities
            .WithBurst()
            .WithAll<RegimentTag, RegimentUnitSelectedTag>()
            .WithNone<RegimentSelectedTag>()
            .ForEach((Entity regimentSelected, int entityInQueryIndex, in DynamicBuffer<Child> unitChild) =>
            {
                for (int i = 0; i < unitChild.Length; i++)
                {
                    if (!HasComponent<SelectedUnitTag>(unitChild[i].Value))
                    {
                        ecb.AddComponent<SelectedUnitTag>(entityInQueryIndex, unitChild[i].Value);
                        ecb.AddComponent<UnitNeedHighlightTag>(entityInQueryIndex, unitChild[i].Value);
                    }
                }
                ecb.AddComponent<RegimentSelectedTag>(entityInQueryIndex, regimentSelected);
                ecb.RemoveComponent<RegimentUnitSelectedTag>(entityInQueryIndex, regimentSelected);
            }).WithName("RegimentSelected")
            .Schedule();
    }
    #endregion Select whole regiment
    /// <summary>
    /// PROBLEME ICI
    /// </summary>
    /// <param name="ecb"></param>
    /*
    #region Highlight
    public void HighlightEnable(EntityCommandBuffer.ParallelWriter ecb)
    {
        Debug.Log("HighlightEnable");
        Entities
            //.WithBurst()
            .WithAll<SelectedUnitTag, UnitNeedHighlightTag, UnitTag>()
            .WithEntityQueryOptions(EntityQueryOptions.IncludeDisabled)
            .ForEach((Entity UnitSelected, int entityInQueryIndex, in DynamicBuffer<LinkedEntityGroup> linkedEntity) =>
            {
                
                for (int i = 1; i < linkedEntity.Length; i++)
                {
                    //Debug.Log("gotit " + linkedEntity[i]);
                    if (HasComponent<HighlightTag>(linkedEntity[i].Value))
                    {
                        ecb.RemoveComponent<Disabled>(entityInQueryIndex, linkedEntity[i].Value);
                    }
                }
                
                //ecb.RemoveComponent<Disabled>(entityInQueryIndex, linkedEntity[1].Value);
                ecb.RemoveComponent<UnitNeedHighlightTag>(entityInQueryIndex, UnitSelected);
            }).Schedule();
    }
    #endregion Highlight
    */
}
