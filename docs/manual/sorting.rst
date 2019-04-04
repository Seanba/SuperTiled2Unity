Sorting with SuperTile2Unity
============================

At the map import level, SuperTiled2Unity has two options for sorting the layers (and objects) in your `Tiled Map Editor <https://www.mapeditor.org/>`__ file.

.. figure:: img/sort/sorting-options.png

**Sorting Modes**

.. csv-table::

   "Stacked", "Default sorting. Matches the rendering order of layers and objects in Tiled."
   "Custom Sort Axis", "Sorting is performed with the help of a Custom Sort Axis (a setting in Unity)."

:code:`Stacked` is a good default for side-scroller games where players and other game objects do not move about the map in ways that change their rendering order.
Overhead-style games may prefer to use the :code:`Custom Sort Axis` setting.
This takes a little more work but will be necessary if you need the rendering order of game objets against tiles to be dynamic.

.. figure:: img/sort/example-sorting.png
   
   The example that comes with SuperTiled2Unity uses a Custom Sort Axis so that our player can be rendered either in front of or behind these columns depending on his current y-position.

How SuperTiled2Unity Implements Sorting
---------------------------------------

In Unity, render order of sprite and tile assets is generally handled through two settings on the `Sprite Renderer <https://docs.unity3d.com/Manual/class-SpriteRenderer.html>`__
and `Tilemap Renderer <https://docs.unity3d.com/Manual/class-TilemapRenderer.html>`__ components:

.. csv-table::

   "Sorting Layer", "Name of sorting layer. See the `Tag Manager <https://docs.unity3d.com/Manual/class-TagManager.html>`__ to manage these."
   "Order in Layer", "How the renderer is sorted in the named layer."

.. tip::
   Objects to be dynamically sorted by a Custom Sort Axis will need to have the same :code:`Sorting Layer` and :code:`Order in Layer` values of the tiles they are sorting against.

SuperTiled2Unity performs sorting almost primarily through manipulating the :code:`Order in Layer` setting of the prefab components it creates during import.
By default, all tile layers use the Unity's built-in :code:`Default` sorting layer with ever increasing :code:`Order in Layer` values.

.. figure:: img/sort/default-sorting.png
   
   Higher layers in Tiled use higher :code:`Order in Layer` values in Unity so that rendering order is preserved.

Most Unity projects, however, will have several custom :code:`Sorting Layers` that we want a mix of tiles and sprites to share.
In these cases, a specifically-named custom property, :code:`unity:SortingLayer`, will direct SuperTiled2Unity further on how sorting fields are assigned.

.. figure:: img/sort/tagman-sort-player.png

.. figure:: img/sort/tiled-custom-prop-player.png

This will result in our :code:`Objects` tile layer breaking the chain of :code:`Default` sorting layers.

.. figure:: img/sort/default-player-sorting.png

Note that the :code:`Clouds` layer will still be rendered on top of the :code:`Objects` layer.
Users may wish to use yet another :code:`unity:SortingLayer` for clouds to make it more explicit that these objects are drawn on top of other tiles and sprites.

.. figure:: img/sort/tiled-custom-prop-sky.png

.. figure:: img/sort/default-player-sky-sorting.png
