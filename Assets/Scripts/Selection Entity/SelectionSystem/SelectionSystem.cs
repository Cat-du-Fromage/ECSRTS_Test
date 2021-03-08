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
[UpdateInGroup(typeof(InitializationSystemGroup))]
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
    BeginSimulationEntityCommandBufferSystem EndInit_ECB;

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
        base.OnCreate();
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        BeginInit_ECB = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        EndInit_ECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
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
                        HighlightDeselectDisable(EndInitecb);
                    }
                    _entityManager.AddComponent<SelectedUnitTag>(_UnitHit);
                    _entityManager.AddComponent<UnitNeedHighlightTag>(_UnitHit);
                    RegUnit = _UnitHit != Entity.Null ? _entityManager.GetComponentData<Parent>(_UnitHit).Value : Entity.Null; //Find the Parent/Regiment Entity of the Unit

                    _entityManager.AddComponent<RegimentUnitSelectedTag>(RegUnit);
                    SelectWholeRegiment(BeginInitecb);


                    //HighlightSelectEnable(EndInitecb);
                    //World.GetOrCreateSystem<EnableHighlight>().Update();
                }
                else
                {
                    // NO TARGET
                    //DeselectALL(BeginInitecb);
                    HighlightDeselectDisable(EndInitecb);
                }
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

                HighlightSelectEnable(EndInitecb);
            }
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
                {
                    ecb.RemoveComponent<RegimentSelectedTag>(entityInQueryIndex, selected);
                    Debug.Log("REmove SelectRegimentTag");
                }
                else
                {
                    ecb.RemoveComponent<SelectedUnitTag>(entityInQueryIndex, selected);
                    ecb.AddComponent<UnitNoNeedHighlightTag>(entityInQueryIndex, selected);
                    Debug.Log("REmove SelectUnitTag");
                }
            }).ScheduleParallel();
        //BeginInit_ECB.AddJobHandleForProducer(this.Dependency);
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
                        UnityEngine.Debug.Log("REGIMENTUNIT PASS");
                    }
                }
                ecb.AddComponent<RegimentSelectedTag>(entityInQueryIndex, regimentSelected);
                ecb.RemoveComponent<RegimentUnitSelectedTag>(entityInQueryIndex, regimentSelected);
            }).ScheduleParallel();
        BeginInit_ECB.AddJobHandleForProducer(this.Dependency);
    }
    #endregion Select whole regiment
    /// <summary>
    /// PROBLEME ICI
    /// </summary>
    /// <param name="ecb"></param>
    
    #region Highlight Enable
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
        //BeginInit_ECB.AddJobHandleForProducer(this.Dependency);
    }
    #endregion Highlight Enable


    #region Select and Highlight Enable
    public void HighlightSelectEnable(EntityCommandBuffer.ParallelWriter ecb)
    {
        Debug.Log("HighlightSelectEnable Enter");
        JobHandle RegimentSelectWhole =
            Entities
                .WithName("Regimentwholeselect")
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
                            Debug.Log("HighlightSelectEnable REGIMENTSELECTED");
                        }
                        else
                        {
                            Debug.Log("HighlightSelectEnable false");
                        }
                    }
                    ecb.AddComponent<RegimentSelectedTag>(entityInQueryIndex, regimentSelected);
                    ecb.RemoveComponent<RegimentUnitSelectedTag>(entityInQueryIndex, regimentSelected);
                    
                }).Schedule(this.Dependency);
        BeginInit_ECB.AddJobHandleForProducer(RegimentSelectWhole);
        //EndInit_ECB.AddJobHandleForProducer(Dependency);
        Debug.Log("HighlightSelectEnable Enter2");
        EntityQuery querytest = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<UnitNeedHighlightTag>());
        Debug.Log(querytest.CalculateEntityCount());
            JobHandle EnableHighlight =
               Entities
                   .WithName("showHighlight")
                   .WithBurst()
                   .WithAll<UnitTag, UnitNeedHighlightTag>()
                   //.WithEntityQueryOptions(EntityQueryOptions.IncludeDisabled)
                   .ForEach((Entity UnitSelected, int entityInQueryIndex, in DynamicBuffer<Child> child) =>
                   {
                       Debug.Log("HighlightSelectEnable UnitSelected");
                       for (int i = 0; i < child.Length; i++)
                       {
                           if (HasComponent<HighlightTag>(child[0].Value))
                           {
                               Debug.Log("HighlightSelectEnable UnitSelected PASS");
                               ecb.RemoveComponent<Disabled>(entityInQueryIndex, child[i].Value);
                           }
                           ecb.RemoveComponent<UnitNeedHighlightTag>(entityInQueryIndex, UnitSelected);
                       }
                    //ecb.RemoveComponent<UnitNeedHighlightTag>(entityInQueryIndex, UnitSelected);
                }).Schedule(RegimentSelectWhole);
            //BeginInit_ECB.AddJobHandleForProducer(RegimentSelectWhole);
            EndInit_ECB.AddJobHandleForProducer(EnableHighlight);
            EnableHighlight.Complete();

        /*
        JobHandle EnableHighlight =
            Entities
                .WithName("showHighlight")
                .WithBurst()
                .WithAll<UnitTag, UnitNeedHighlightTag>()
                //.WithEntityQueryOptions(EntityQueryOptions.IncludeDisabled)
                .ForEach((Entity UnitSelected, int entityInQueryIndex, in DynamicBuffer<Child> child) =>
                {
                        Debug.Log("HighlightSelectEnable UnitSelected");
                        for (int i = 0; i < child.Length; i++)
                        {
                            if (HasComponent<HighlightTag>(child[0].Value))
                            {
                                Debug.Log("HighlightSelectEnable UnitSelected PASS");
                                ecb.RemoveComponent<Disabled>(entityInQueryIndex, child[i].Value);
                            }
                        ecb.RemoveComponent<UnitNeedHighlightTag>(entityInQueryIndex, UnitSelected);
                        }
                    //ecb.RemoveComponent<UnitNeedHighlightTag>(entityInQueryIndex, UnitSelected);
                }).Schedule(RegimentSelectWhole);
                //BeginInit_ECB.AddJobHandleForProducer(RegimentSelectWhole);
                EndInit_ECB.AddJobHandleForProducer(EnableHighlight);
                EnableHighlight.Complete();
        */
    }
    #endregion Select and Highlight Enable


    #region Deselect and Highlight Disable
    public void HighlightDeselectDisable(EntityCommandBuffer.ParallelWriter ecb)
    {
        JobHandle Deselect =
            Entities
                .WithAny<SelectedUnitTag, RegimentSelectedTag>()
                .WithBurst()
                .ForEach((Entity selected, int entityInQueryIndex) =>
                {
                    if (HasComponent<RegimentSelectedTag>(selected))
                    {
                        ecb.RemoveComponent<RegimentSelectedTag>(entityInQueryIndex, selected);
                    }
                    else
                    {
                        ecb.RemoveComponent<SelectedUnitTag>(entityInQueryIndex, selected);
                        ecb.AddComponent<UnitNoNeedHighlightTag>(entityInQueryIndex, selected);
                    }
                }).ScheduleParallel(Dependency);
        //Dependency.Complete();
        //BeginInit_ECB.AddJobHandleForProducer(this.Dependency);
        //EndInit_ECB.AddJobHandleForProducer(this.Dependency);

        JobHandle HideHighlight =
            Entities
               .WithName("HideHighlight")
               .WithBurst()
               .WithAll<UnitNoNeedHighlightTag, UnitTag>()
               .ForEach((Entity UnitSelected, int entityInQueryIndex, in DynamicBuffer<Child> child) =>
               {
                   for (int i = 0; i < child.Length; i++)
                   {
                        if (HasComponent<HighlightTag>(child[i].Value))
                        {
                            ecb.AddComponent<Disabled>(entityInQueryIndex, child[i].Value);
                        }
                   }

                   //ecb.RemoveComponent<Disabled>(entityInQueryIndex, linkedEntity[1].Value);
                   ecb.RemoveComponent<UnitNoNeedHighlightTag>(entityInQueryIndex, UnitSelected);
               }).Schedule(Deselect);
            //BeginInit_ECB.AddJobHandleForProducer(this.Dependency);
            EndInit_ECB.AddJobHandleForProducer(Deselect);
            //this.Dependency = HideHighlight;
            HideHighlight.Complete();
    }
    #endregion Deselect and Highlight Disable

}
