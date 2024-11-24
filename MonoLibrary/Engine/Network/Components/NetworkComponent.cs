using MonoLibrary.Engine.Components.Interfaces;
using MonoLibrary.Engine.Network.Datas;
using MonoLibrary.Engine.Objects;

using System.Diagnostics;

namespace MonoLibrary.Engine.Network.Components;

[DebuggerDisplay("{ToString()}")]
public abstract class NetworkComponent(GameObject owner) : NetVarContainer, IUpdateComponent
{
    public NetworkIdentityComponent Identity { get; private set; }

    public GameObject Owner { get; } = owner;

    internal void Initialize(NetworkIdentityComponent identity)
    {
        Identity = identity;

        Init();

        if (identity.IsServer)
            ServerInit();
        else
            ClientInit();
    }

    /// <summary>
    /// Called when this <see cref="NetworkComponent"/> has received its <see cref="Identity"/>. <br/>
    /// On server, called before first serialization. <br/>
    /// On client, called after first serialization.
    /// </summary>
    protected virtual void Init() { }

    /// <summary>
    /// Called only on server.
    /// </summary>
    protected virtual void ServerInit() { }

    /// <summary>
    /// Called only on client.
    /// </summary>
    protected virtual void ClientInit() { }

    public void Update(float time)
    {
        if (Identity is null)
            return;

        if (Identity.IsLocalPlayer && !Identity.IsServer)
            UpdatePlayer(time);
        else
            UpdateServer(time);
    }

    /// <summary>
    /// Run only on client side, when <see cref="NetworkIdentityComponent.IsLocalPlayer"/> is <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// Should only deal with local user inputs.
    /// </remarks>
    /// <param name="time"></param>
    protected virtual void UpdatePlayer(float time) { }

    /// <summary>
    /// Run when <see cref="UpdatePlayer(float)"/> is not run.
    /// </summary>
    /// <remarks>
    /// Should contain all the update logic, shared both by client and server, without input dependency.
    /// </remarks>
    /// <param name="time"></param>
    protected virtual void UpdateServer(float time) { }

    public override string ToString()
    {
        return GetType().Name;
    }
}
