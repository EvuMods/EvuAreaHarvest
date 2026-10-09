using System.Diagnostics;
using UnityEngine;

namespace EvuAreaHarvest;

internal sealed class HarvestHighlight
{
    const float ScanInterval = 0.25f;

    readonly RangeRing _range;
    readonly HarvestMarkers _markers;
    readonly Stopwatch _clock = new Stopwatch();
    float _nextScan = -1f;
    float _nextReport = -1f;
    bool _shown;
    int _frames;
    long _scanMax;
    long _scanSum;
    long _markerMax;
    long _markerSum;
    int _scans;
    int _lastVisited;
    int _lastFound;
    long _ringMax;
    long _ringSum;

    public HarvestHighlight(Transform parent)
    {
        _range = new RangeRing(parent);
        _markers = new HarvestMarkers(parent);
    }

    public void Tick(Player player, bool show, float range, bool pickGuarded)
    {
        if (!show)
        {
            if (_shown)
            {
                Hide();
                _shown = false;
                _nextScan = -1f;
                _nextReport = -1f;
                ResetTimings();
            }

            return;
        }

        _shown = true;
        var log = Plugin.Settings.LogHighlightTimings.Value;
        if (!log && (_frames > 0 || _scans > 0))
        {
            ResetTimings();
        }
        var origin = player.transform.position;
        var scan = Time.time >= _nextScan;
        if (scan)
        {
            if (log)
            {
                _clock.Restart();
            }

            var found = HarvestScan.Collect(origin, range, pickGuarded, out var visited);
            if (log)
            {
                _clock.Stop();
                Note(ref _scanMax, ref _scanSum, _clock.ElapsedTicks);
                _scans++;
                _lastVisited = visited;
                _lastFound = found.Count;
                _clock.Restart();
            }

            _markers.Show(found);
            if (log)
            {
                _clock.Stop();
                Note(ref _markerMax, ref _markerSum, _clock.ElapsedTicks);
            }

            _nextScan = Time.time + ScanInterval;
        }

        if (log)
        {
            _clock.Restart();
        }

        _range.Show(origin, range, scan);
        if (log)
        {
            _clock.Stop();
            Note(ref _ringMax, ref _ringSum, _clock.ElapsedTicks);
            _frames++;
            if (_nextReport < 0f)
            {
                _nextReport = Time.time + 1f;
            }
            else if (Time.time >= _nextReport)
            {
                Report();
                ResetTimings();
                _nextReport = Time.time + 1f;
            }
        }
    }

    public void Hide()
    {
        _range.Hide();
        _markers.Hide();
    }

    public void Destroy()
    {
        Hide();
        GroundLine.DestroyMaterial();
    }

    static void Note(ref long max, ref long sum, long ticks)
    {
        sum += ticks;
        if (ticks > max)
        {
            max = ticks;
        }
    }

    void Report()
    {
        var frequency = (double)Stopwatch.Frequency;
        var scanAvg = _scans == 0 ? 0d : _scanSum / (double)_scans / frequency * 1000d;
        var markerAvg = _scans == 0 ? 0d : _markerSum / (double)_scans / frequency * 1000d;
        var ringAvg = _frames == 0 ? 0d : _ringSum / (double)_frames / frequency * 1000d;
        Plugin.Log.LogInfo(
            "highlight: scan " + (_scanMax / frequency * 1000d).ToString("0.0") + " ms max ("
            + _lastFound + " in range of " + _lastVisited + "), markers "
            + (_markerMax / frequency * 1000d).ToString("0.0") + " ms, ring "
            + (_ringMax / frequency * 1000d).ToString("0.0") + " ms/frame max, " + _frames + " frames"
            + " (avg scan " + scanAvg.ToString("0.0") + ", markers " + markerAvg.ToString("0.0") + ", ring " + ringAvg.ToString("0.0") + ")");
    }

    void ResetTimings()
    {
        _frames = 0;
        _scans = 0;
        _scanMax = 0;
        _scanSum = 0;
        _markerMax = 0;
        _markerSum = 0;
        _ringMax = 0;
        _ringSum = 0;
    }
}
