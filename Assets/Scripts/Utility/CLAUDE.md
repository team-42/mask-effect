# Utility Scripts Documentation

## Overview

This document provides documentation for general utility scripts used across the 'Mask Effect' project.

## Billboard.cs

### Purpose

The `Billboard.cs` script ensures that a GameObject always faces the main camera, regardless of its own rotation. This is particularly useful for world-space UI elements, such as hovering icons, that need to maintain a consistent orientation towards the viewer.

### Usage

Attach this script to any GameObject that should always face the camera. The script automatically finds the main camera in the scene (ensure your camera is tagged "MainCamera").

### Integration

This script is used by `NetworkMask.cs` to make the hovering mask icons always face the camera.
