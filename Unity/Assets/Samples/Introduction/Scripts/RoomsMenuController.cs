﻿using System.Collections.Generic;
using System.Linq;
using Ubiq.Rooms;
using UnityEngine;

namespace Ubiq.Samples
{
    public class RoomsMenuController : MonoBehaviour
    {
        public float roomListRefreshInterval = 2.0f;
        public RoomClient roomClient;
        public RoomsMenuControl joinedControl;
        public Transform controlsContainer;
        public GameObject controlPrefab;
        public RoomsMenuInfoPanel infoPanel;

        private float lastDiscoverTime;
        private List<RoomsMenuControl> controls;
        private List<RoomInfo> lastRoomArgs;

        private void Awake()
        {
            controls = GetComponentsInChildren<RoomsMenuControl>().ToList();
        }

        private void Start()
        {
            roomClient.OnRoomsAvailable.AddListener(OnRoomsAvailable);
            roomClient.OnJoinedRoom.AddListener(OnJoinedRoom);
            UpdateAvailableRooms(); // This clears the control list before users see the development entries
        }

        private RoomsMenuControl InstantiateControl () {
            var go = GameObject.Instantiate(controlPrefab, controlsContainer);
            return go.GetComponent<RoomsMenuControl>();
        }

        private void OnJoinedRoom(RoomInfo room)
        {
            UpdateAvailableRooms();

            // Immediately ask for a refresh - maybe room we left is now empty
            roomClient.DiscoverRooms();
        }

        private void OnRoomsAvailable(List<RoomInfo> rooms)
        {
            lastRoomArgs = rooms;
            UpdateAvailableRooms();
        }

        private void UpdateAvailableRooms() {
            var rooms = lastRoomArgs;

            if (rooms == null)
            {
                for (int i = 0; i < controls.Count; i++)
                {
                    Destroy(controls[i].gameObject);
                }
                controls.Clear();
                return;
            }

            if (roomClient.JoinedRoom) {    // Update joined room
                if (joinedControl != null)
                {
                    joinedControl.gameObject.SetActive(true);
                    joinedControl.Bind(roomClient.Room.GetRoomInfo(), roomClient);
                }
            }

            int controlI = 0;
            for (int roomI = 0; roomI < rooms.Count; controlI++,roomI++)
            {
                if (roomClient.JoinedRoom &&
                    roomClient.Room.UUID == rooms[roomI].UUID)
                {
                    // Skip room we are currently in - handled elsewhere
                    controlI--;
                    continue;
                }

                if (controls.Count <= controlI) {
                    controls.Add(InstantiateControl());
                }

                controls[controlI].Bind(rooms[roomI], roomClient);
            }

            while (controls.Count > controlI) {
                Destroy(controls[controlI].gameObject);
                controls.RemoveAt(controlI);
            }
        }

        public void Create()
        {
            roomClient.JoinNew("Room",true);
        }

        public void Join(RoomsMenuControl control)
        {
            roomClient.Join(control.room.Joincode);
        }

        public void Select(RoomsMenuControl control)
        {
            infoPanel.Bind(control.room, roomClient);
        }

        private void Update()
        {
            if(Mathf.Abs(lastDiscoverTime - Time.time) > roomListRefreshInterval)
            {
                lastDiscoverTime = Time.time;
                roomClient.DiscoverRooms();
            }
        }
    }
}