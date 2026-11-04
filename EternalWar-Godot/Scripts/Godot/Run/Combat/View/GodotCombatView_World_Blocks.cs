// Original Authors - Wyatt Senalik
using EternalWar.Engine.Godot.Run.Combat.Nodes;
using EternalWar.Engine.Godot.Run.Combat.Resources;
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using Godot;
using GodotExtensions;
using System.Collections.Generic;
using System.Linq;
using NumericsVector3 = System.Numerics.Vector3;

namespace EternalWar.Engine.Godot.Run.Combat.View
{
    public sealed class GodotCombatView_World_Blocks
    {
        private readonly Dictionary<Vector3I, GodotCombatBlockView> _blocksMap = new();
        private readonly GodotGameVisualDatabase _visualDatabase;
        private readonly GodotCombatNodeCollection _combatNodeCollectionInstance;
        private readonly int _blockColorId;

        public GodotCombatView_World_Blocks(GodotGameVisualDatabase visualDatabase, GodotCombatNodeCollection combatNodeCollectionInstance)
        {
            _visualDatabase = visualDatabase;
            _combatNodeCollectionInstance = combatNodeCollectionInstance;
            _blockColorId = RenderUtil.CheckoutColorId();
        }

        public void CreateBlock(BlockInfo blockInfo, NumericsVector3 position)
        {
            string blockDefinitionId = blockInfo.DefinitionId;
            GridPosition gridPosition = blockInfo.Position;
            Rotation_Restricted_Four restrictedRotation = blockInfo.Rotation;
            int height = blockInfo.Height;

            GodotBlockVisualResource blockVisualResource = _visualDatabase.GetBlock(blockDefinitionId);

            PackedScene visualPrefab = blockVisualResource.VisualPrefab;
            Node3D blockVisualInstance = visualPrefab.Instantiate<Node3D>();
            blockVisualInstance.RotationDegrees = new Vector3(0.0f, restrictedRotation.GetEulerAngle(), 0.0f);

            Node blocksParentNode = _combatNodeCollectionInstance.BlocksParentNode;
            blocksParentNode.AddChild(blockVisualInstance);

            // Position
            blockVisualInstance.GlobalPosition = position.ToGodot();

            // Rotation
            float rotationAngle = restrictedRotation.GetEulerAngle();
            Vector3 rotationDegrees = new(0.0f, rotationAngle, 0.0f);
            blockVisualInstance.GlobalRotationDegrees = rotationDegrees;

            GodotCombatBlockView blockView = new(blockVisualInstance);

            // Material
            InitializeBlockMaterial(blockView, _blockColorId);

            Vector3I key = CreateKey(gridPosition, height);
            _blocksMap.Add(key, blockView);
        }

        public void DeleteAllBlocks()
        {
            foreach (GodotCombatBlockView blockView in _blocksMap.Values)
            {
                Node3D visualInstance = blockView.VisualInstance;
                visualInstance.QueueFree();
            }
            _blocksMap.Clear();
        }

        private static void InitializeBlockMaterial(GodotCombatBlockView blockView, int colorId)
        {
            Node3D blockVisualInstance = blockView.VisualInstance;
            IEnumerable<MeshInstance3D> meshInstances = blockVisualInstance.FindChildren("*", type: nameof(MeshInstance3D), recursive: true).Cast<MeshInstance3D>();
            foreach (MeshInstance3D singleMeshInstance in meshInstances)
            {
                Mesh mesh = singleMeshInstance.Mesh;
                for (int surfaceIndex = 0; surfaceIndex < mesh.GetSurfaceCount(); ++surfaceIndex)
                {
                    if (singleMeshInstance.GetActiveMaterial(surfaceIndex) is not ShaderMaterial originalToonMaterial)
                        continue;
                    ShaderMaterial outlineMaterial = RenderUtil.CreateNewOutlineMaterialWithExistingId(originalToonMaterial, colorId);
                    singleMeshInstance.SetSurfaceOverrideMaterial(surfaceIndex, outlineMaterial);
                }
            }
        }

        private static Vector3I CreateKey(GridPosition gridPosition, int height)
        {
            Vector2Int gridPositionVector = gridPosition.Position;
            return new Vector3I(gridPositionVector.X, height, gridPositionVector.Y);
        }
    }
}
