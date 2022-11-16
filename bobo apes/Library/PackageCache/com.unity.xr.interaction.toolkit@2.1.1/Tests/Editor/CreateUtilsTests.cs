using NUnit.Framework;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools.Utils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Tests;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace UnityEditor.XR.Interaction.Toolkit.Editor.Tests
{
    [TestFixture]
    class CreateUtilsTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Since this is an Edit Mode test, make sure all scenes are empty
            // to prevent components being found in the loaded scene when this test
            // is manually run in the Editor with a scene already open.
            TestUtilities.DestroyAllSceneObjects();
        }

        [TearDown]
        public void TearDown()
        {
            TestUtilities.DestroyAllSceneObjects();
        }

        [Test]
        public void CreateXROriginForVR_CreatesSuccessfully()
        {
            CreateUtils.CreateXROriginForVR(null);
            var origin = Object.FindObjectOfType<XROrigin>();
            Assert.IsTrue(origin != null);
            Assert.IsTrue(origin.Camera != null);
        }

        [Test]
        public void UndoRedoCreateXROrigin_WorksWithNoErrors()
        {
            CreateUtils.CreateXROriginDeviceBased(null);
            var origin = Object.FindObjectOfType<XROrigin>();
            var cameraBgColor = origin.Camera.backgroundColor;
            var originPosition = origin.transform.position;

            Undo.PerformUndo();
            origin = Object.FindObjectOfType<XROrigin>();
            Assert.IsTrue(origin == null);
            
            Undo.PerformRedo();
            origin = Object.FindObjectOfType<XROrigin>();
            Assert.IsTrue(origin != null);
            Assert.That(origin.Camera.backgroundColor, Is.EqualTo(cameraBgColor).Using(ColorEqualityComparer.Instance));
            Assert.That(origin.transform.position, Is.EqualTo(originPosition).Using(Vector3ComparerWithEqualsOperator.Instance));
            
            Undo.PerformUndo();
            Undo.PerformRedo();

            origin = Object.FindObjectOfType<XROrigin>();
            Assert.IsTrue(origin != null);
            Assert.That(origin.Camera.backgroundColor, Is.EqualTo(cameraBgColor).Using(ColorEqualityComparer.Instance));
            Assert.That(origin.transform.position, Is.EqualTo(originPosition).Using(Vector3ComparerWithEqualsOperator.Instance));
        }

        [Test]
        public void CreateRayInteractorActionBased_CreatesSuccessfully()
        {
            CreateUtils.CreateRayInteractorActionBased(null);
            var rayInteractor = Object.FindObjectOfType<XRRayInteractor>();
            Assert.IsTrue(rayInteractor != null);
        }

        [Test]
        public void UndoRedoCreateRayInteractor_WorksWithNoErrors()
        {
            var parentTransform = new GameObject().transform;
            parentTransform.position = new Vector3(5f, 5f, 5f);
            CreateUtils.CreateRayInteractorDeviceBased(new MenuCommand(parentTransform.gameObject));
            
            Undo.PerformUndo();
            var rayInteractor = Object.FindObjectOfType<XRRayInteractor>();
            Assert.IsTrue(rayInteractor == null);
            
            Undo.PerformRedo();
            rayInteractor = Object.FindObjectOfType<XRRayInteractor>();
            Assert.IsTrue(rayInteractor != null);
            var rayTransform = rayInteractor.transform;
            Assert.That(rayTransform.parent, Is.EqualTo(parentTransform));
            Assert.That(rayTransform.localPosition, Is.EqualTo(Vector3.zero).Using(Vector3ComparerWithEqualsOperator.Instance));
            
            var interactionManager = Object.FindObjectOfType<XRInteractionManager>();
            Assert.IsTrue(interactionManager != null);
        }

        [Test]
        public void CreateXRUICanvas_CreatesSuccessfully()
        {
            CreateUtils.CreateXRUICanvas(null);
            var canvas = Object.FindObjectOfType<Canvas>();
            var inputModule = Object.FindObjectOfType<XRUIInputModule>();
            Assert.IsTrue(canvas != null);
            Assert.IsTrue(inputModule != null);
        }

        [Test]
        public void UndoCreateXRUICanvas_RestoresStandaloneInputModule()
        {
            var eventSystemGO = ObjectFactory.CreateGameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule));
            Undo.IncrementCurrentGroup();
            
            CreateUtils.CreateXRUICanvas(null);
            Assert.IsTrue(eventSystemGO.GetComponent<StandaloneInputModule>() == null);
            Assert.IsTrue(eventSystemGO.GetComponent<XRUIInputModule>() != null);
            
            Undo.PerformUndo();
            Assert.IsTrue(eventSystemGO.GetComponent<StandaloneInputModule>() != null);
            Assert.IsTrue(eventSystemGO.GetComponent<XRUIInputModule>() == null);
        }

        [Test]
        public void UndoCreateXRUIEventSystem_DoesNotDestroyReusedExistingUIEventSystem()
        {
            // This tests to make sure the GameObject created with UI > Event System
            // is not destroyed when the XR > UI Event System reuses the Event System.
            // It should only replace the StandaloneInputModule with XRUIInputModule.

            var eventSystemGO = ObjectFactory.CreateGameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule));
            Undo.IncrementCurrentGroup();

            CreateUtils.CreateXRUIEventSystem(null);
            Undo.PerformUndo();

            Assert.IsTrue(eventSystemGO != null);
            Assert.IsTrue(eventSystemGO.GetComponent<StandaloneInputModule>() != null);
            Assert.IsTrue(eventSystemGO.GetComponent<XRUIInputModule>() == null);

            Undo.PerformRedo();

            Assert.IsTrue(eventSystemGO != null);
            Assert.IsTrue(eventSystemGO.GetComponent<StandaloneInputModule>() == null);
            Assert.IsTrue(eventSystemGO.GetComponent<XRUIInputModule>() != null);
        }

        [Test]
        public void UndoCreateXRUIEventSystem_DoesNotDestroyExistingXRUIEventSystem()
        {
            // This tests to make sure the GameObject created with XR > UI Event System
            // is not destroyed upon undo when the menu item is executed again.
            // It should only select the existing EventSystem GameObject, not destroy it.
            // This ensures it matches the behavior of the UI > Event System menu item.

            var eventSystemGO = ObjectFactory.CreateGameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(XRUIInputModule));
            Undo.IncrementCurrentGroup();

            CreateUtils.CreateXRUIEventSystem(null);
            Undo.PerformUndo();

            Assert.IsTrue(eventSystemGO != null);
            Assert.IsTrue(eventSystemGO.GetComponent<XRUIInputModule>() != null);

            Undo.PerformRedo();

            Assert.IsTrue(eventSystemGO != null);
            Assert.IsTrue(eventSystemGO.GetComponent<XRUIInputModule>() != null);
        }
    }
}
