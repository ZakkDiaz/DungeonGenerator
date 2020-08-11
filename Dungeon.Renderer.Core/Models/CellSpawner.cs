using System;
using System.Collections.Generic;
using System.Text;
using DungeonRenderer.Models;
using UnityEngine;

namespace Dungeon.Renderer.Core.Models
{
    public class CellSpawner : MonoBehaviour
    {
        public GameObject cellObject;
        private static IDungeonGenerator gen = new CellGenerator();
        private static readonly int size = 10;
        private static readonly int prune = 2;

        private ITile currentTile;

        void Start()
        {
            gen.GenerateDungeon("0", size, prune);
            currentTile = gen.GetEntrance();

            for (var i = 0; i < size; i++)
            {
                for (var ii = 0; ii < size; ii++)
                {
                    var tmpTile = gen.GetTile(i, ii);
                    Instantiate(cellObject,
                        new Vector3(transform.position.x + tmpTile.GetLocation().X * 43f, 0, transform.position.y + tmpTile.GetLocation().Y * 43f),
                        new Quaternion(0, 90, 90, 0));
                }
            }

        }


        public float targetTime = 3f;
        void Update()
        {

            targetTime -= Time.deltaTime;

            if (targetTime <= 3f)
            {
                DoMove();
            }

        }

        private void DoMove()
        {

        }
    }
}
