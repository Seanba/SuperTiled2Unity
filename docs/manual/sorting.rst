Sorting with SuperTile2Unity
============================

At the map import level, SuperTiled2Unity has two options for sorting the layers (and objects) in your `Tiled Map Editor <https://www.mapeditor.org/>`__ file.

.. figure:: img/sorting-options.png

**Sorting Modes**

.. csv-table::

   "Stacked", "Default sorting. Matches the rendering order of layers and objects in Tiled."
   "Custom Sort Axis", "Sorting is performed with the help of a Custom Sort Axis (a setting in Unity)."

:code:`Stacked` is a good default for side-scroller games where players and other game objects do not move about the map in ways that change their rendering order.
Overhead-style games may prefer to use the :code:`Custom Sort Axis` setting. This generally takes more work but will be necessary if you need the rendering order of game objets against tiles to be dynamic.

.. figure:: img/example-sorting.png
   
   The example that comes with SuperTiled2Unity uses a Custom Sort Axis so that our player can be rendered either in front of or behind these columns depending on his current y-position.

