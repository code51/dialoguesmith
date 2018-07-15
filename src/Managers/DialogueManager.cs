using System;
using DialogueSmith.Runtime;
using UnityEngine;

namespace DialogueSmith.Managers
{
    public class DialogueManager
    {
        protected EntityManager entityManager;
        protected System.Random random;

        public DialogueManager(EntityManager entityManager)
        {
            this.entityManager = entityManager;
            this.random = new System.Random();
        }

        public DialogueManager(EntityManager entityManager, System.Random random)
        {
            this.random = random;
        }

        public DialogueManager()
        {
            this.random = new System.Random();
            this.entityManager = null;
        }

        public DialogueManager(System.Random random)
        {
            this.random = random;
            this.entityManager = null;
        }

        public RuntimeBuilder Build(TextAsset text)
        {
            return new RuntimeBuilder(entityManager.LoadTree(text), random);
        }

        public RuntimeBuilder Build(TextAsset text, System.Random random)
        {
            return new RuntimeBuilder(entityManager.LoadTree(text), random);
        }

        public RuntimeBuilder Build(string name)
        {
            if (entityManager == null)
                throw new Exception("Entity manager is not available");

            return new RuntimeBuilder(entityManager.LoadTree(name), random);
        }

        public RuntimeBuilder Build(string name, System.Random random)
        {
            if (entityManager == null)
                throw new Exception("Entity manager is not available");

            return new RuntimeBuilder(entityManager.LoadTree(name), random);
        }
    }
}
