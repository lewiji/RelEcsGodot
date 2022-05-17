using Godot;
using Godot.Collections;

namespace RelEcs.Godot
{
    public struct Root
    {
        public Node Node;
        public Root(Node node) => Node = node;
    }

    // wraps a godot node into an ecs component
    public struct Node<T> where T : Node
    {
        public T Value;
        public Node(T value) => Value = value;
    }

    // wraps an ecs object into a godot variant
    public class Marshallable<T> : Object
    {
        public T Value;
        public Marshallable(T value) => Value = value;
    }
    
    public static class WorldExtensions
    {
        public static void SpawnRecursively(this World world, Node node)
        {
            world.Spawn(node);
            
            foreach (Node child in node.GetChildren())
            {
                if (child.GetChildCount() == 0) continue;
                
                world.SpawnRecursively(child);
            }
        }
        
        public static Entity Spawn(this World world, Node parent)
        {
            var entity = world.Spawn().Add(new Root(parent));

            var nodes = new Array();
            nodes.Add(parent);

            foreach (Node child in parent.GetChildren())
            {
                nodes.Add(child);
            }

            foreach (Node node in nodes)
            {
                var addMethod = typeof(WorldExtensions).GetMethod("AddNodeHandle");
                var addChildMethod = addMethod?.MakeGenericMethod(new[] { node.GetType() });
                addChildMethod?.Invoke(null, new object[] { entity, node });
            }

            return entity;
        }

        public static void AddNodeHandle<T>(Entity entity, T node) where T : Node
        {
            entity.Add(new Node<T>(node));
            node.SetMeta("Entity", new Marshallable<Entity>(entity));
        }
    }
    
    public static class CommandsExtensions
    {
        public static Entity Spawn(this Commands commands, Node parent)
        {
            return commands.World.Spawn(parent);
        }
    }

    public static class EntityExtensions
    {
        public static void DespawnAndFree(this Entity entity)
        {
            if (entity.Has<Root>())
            {
                entity.Get<Root>().Node.QueueFree();
            }
            
            entity.Despawn();
        }
    }
}