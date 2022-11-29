//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.4.4
//     from Assets/ActionMaps/PlayerInputs.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @PlayerInputs : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerInputs()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInputs"",
    ""maps"": [
        {
            ""name"": ""Movement"",
            ""id"": ""c51088cb-d5ed-4803-8d45-a508157667c3"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""efd94aa1-b8b8-49c4-a8e9-7673df0cc0c9"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD"",
                    ""id"": ""905b6a6b-0dec-4995-816a-f3a5772e6a9a"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""eb69145b-48be-4ce3-9b66-6b4f8be670e5"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""d4169058-5cd8-40a3-a811-79b8900b7fed"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""d8f5d09c-76d8-4189-baf2-1e6c58cf48b5"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""fbfe2a20-6e0b-42c3-b9ac-6b065ba7aa98"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        },
        {
            ""name"": ""Capacity"",
            ""id"": ""1ad1369f-3054-4487-84cc-92a46f61278a"",
            ""actions"": [
                {
                    ""name"": ""Capacity1"",
                    ""type"": ""Button"",
                    ""id"": ""69b3dbeb-9d62-4040-8069-29ece82405be"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Capacity2"",
                    ""type"": ""Button"",
                    ""id"": ""60258e78-bf49-4b65-98b6-960321c73b0b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Ultime"",
                    ""type"": ""Button"",
                    ""id"": ""4066f0d7-3890-4e93-b91d-2252531678e5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""dfccd1af-df03-4fdc-990f-4d6028599b7f"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Capacity1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""31ef5a54-20fe-4f1b-a2ab-ff4c933f194a"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Capacity2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d81835f5-f080-4ee4-b222-4b33eac0df33"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Ultime"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Movement
        m_Movement = asset.FindActionMap("Movement", throwIfNotFound: true);
        m_Movement_Move = m_Movement.FindAction("Move", throwIfNotFound: true);
        // Capacity
        m_Capacity = asset.FindActionMap("Capacity", throwIfNotFound: true);
        m_Capacity_Capacity1 = m_Capacity.FindAction("Capacity1", throwIfNotFound: true);
        m_Capacity_Capacity2 = m_Capacity.FindAction("Capacity2", throwIfNotFound: true);
        m_Capacity_Ultime = m_Capacity.FindAction("Ultime", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Movement
    private readonly InputActionMap m_Movement;
    private IMovementActions m_MovementActionsCallbackInterface;
    private readonly InputAction m_Movement_Move;
    public struct MovementActions
    {
        private @PlayerInputs m_Wrapper;
        public MovementActions(@PlayerInputs wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Movement_Move;
        public InputActionMap Get() { return m_Wrapper.m_Movement; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MovementActions set) { return set.Get(); }
        public void SetCallbacks(IMovementActions instance)
        {
            if (m_Wrapper.m_MovementActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_MovementActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_MovementActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_MovementActionsCallbackInterface.OnMove;
            }
            m_Wrapper.m_MovementActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
            }
        }
    }
    public MovementActions @Movement => new MovementActions(this);

    // Capacity
    private readonly InputActionMap m_Capacity;
    private ICapacityActions m_CapacityActionsCallbackInterface;
    private readonly InputAction m_Capacity_Capacity1;
    private readonly InputAction m_Capacity_Capacity2;
    private readonly InputAction m_Capacity_Ultime;
    public struct CapacityActions
    {
        private @PlayerInputs m_Wrapper;
        public CapacityActions(@PlayerInputs wrapper) { m_Wrapper = wrapper; }
        public InputAction @Capacity1 => m_Wrapper.m_Capacity_Capacity1;
        public InputAction @Capacity2 => m_Wrapper.m_Capacity_Capacity2;
        public InputAction @Ultime => m_Wrapper.m_Capacity_Ultime;
        public InputActionMap Get() { return m_Wrapper.m_Capacity; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CapacityActions set) { return set.Get(); }
        public void SetCallbacks(ICapacityActions instance)
        {
            if (m_Wrapper.m_CapacityActionsCallbackInterface != null)
            {
                @Capacity1.started -= m_Wrapper.m_CapacityActionsCallbackInterface.OnCapacity1;
                @Capacity1.performed -= m_Wrapper.m_CapacityActionsCallbackInterface.OnCapacity1;
                @Capacity1.canceled -= m_Wrapper.m_CapacityActionsCallbackInterface.OnCapacity1;
                @Capacity2.started -= m_Wrapper.m_CapacityActionsCallbackInterface.OnCapacity2;
                @Capacity2.performed -= m_Wrapper.m_CapacityActionsCallbackInterface.OnCapacity2;
                @Capacity2.canceled -= m_Wrapper.m_CapacityActionsCallbackInterface.OnCapacity2;
                @Ultime.started -= m_Wrapper.m_CapacityActionsCallbackInterface.OnUltime;
                @Ultime.performed -= m_Wrapper.m_CapacityActionsCallbackInterface.OnUltime;
                @Ultime.canceled -= m_Wrapper.m_CapacityActionsCallbackInterface.OnUltime;
            }
            m_Wrapper.m_CapacityActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Capacity1.started += instance.OnCapacity1;
                @Capacity1.performed += instance.OnCapacity1;
                @Capacity1.canceled += instance.OnCapacity1;
                @Capacity2.started += instance.OnCapacity2;
                @Capacity2.performed += instance.OnCapacity2;
                @Capacity2.canceled += instance.OnCapacity2;
                @Ultime.started += instance.OnUltime;
                @Ultime.performed += instance.OnUltime;
                @Ultime.canceled += instance.OnUltime;
            }
        }
    }
    public CapacityActions @Capacity => new CapacityActions(this);
    public interface IMovementActions
    {
        void OnMove(InputAction.CallbackContext context);
    }
    public interface ICapacityActions
    {
        void OnCapacity1(InputAction.CallbackContext context);
        void OnCapacity2(InputAction.CallbackContext context);
        void OnUltime(InputAction.CallbackContext context);
    }
}
