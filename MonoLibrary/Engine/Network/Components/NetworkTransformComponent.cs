using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;

using MonoLibrary.Engine.Network.Datas;
using MonoLibrary.Engine.Objects;

namespace MonoLibrary.Engine.Network.Components
{
    public class NetworkTransformComponent : NetworkComponent
    {
        private readonly NetVar<Vector2> _position;

        public NetworkTransformComponent(GameObject owner) : base(owner)
        {
            _position = new NetVar<Vector2>(owner.Position);
            AddNetVar(_position);
        }

        private void Position_ValueChanged(Vector2 old, Vector2 @new)
        {
            // TODO => Implement lerp between old and @new
            Owner.Position = @new;
        }

        protected override void ClientInit()
        {
            Owner.Position = _position.Value;
            _position.OnValueChanged += Position_ValueChanged;
        }

        protected override void ServerInit()
        {
            if (Identity.IsServer)
                _position.Value = Owner.Position;
            else
                Owner.Position = _position.Value;
        }

        protected override void UpdatePlayer(float time)
        {
            // TODO => Implement lerp between old and @new
            //Owner.Position = _position.Value;
        }

        protected override void UpdateServer(float time)
        {
            if (Identity.IsServer)
                _position.Value = Owner.Position;
            else
                Owner.Position = _position.Value;
        }
    }
}
