using UnityEngine;
// Meshes generated for selectable characters must be released when switching variants.
public sealed class CharacterMeshOwner:MonoBehaviour {
 public Mesh Mesh;
 void OnDestroy(){if(Mesh){if(Application.isPlaying)Destroy(Mesh);else DestroyImmediate(Mesh);}}
}
