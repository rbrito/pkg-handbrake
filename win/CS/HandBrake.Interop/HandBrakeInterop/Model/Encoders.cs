﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Encoders.cs" company="HandBrake Project (http://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrake.Interop.Model
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;
	using HandBrake.Interop.HbLib;
	using HandBrake.Interop.Model.Encoding;
	using HandBrake.Interop.SourceData;

	public static class Encoders
	{
		private static List<HBAudioEncoder> audioEncoders;
		private static List<HBVideoEncoder> videoEncoders;
		private static List<HBMixdown> mixdowns;
		private static List<int> audioBitrates; 

		/// <summary>
		/// Gets a list of supported audio encoders.
		/// </summary>
		public static List<HBAudioEncoder> AudioEncoders
		{
			get
			{
				if (audioEncoders == null)
				{
					IntPtr encodersPtr = HBFunctions.hb_get_audio_encoders();
					int encoderCount = HBFunctions.hb_get_audio_encoders_count();

					audioEncoders = InteropUtilities.ConvertArray<hb_encoder_s>(encodersPtr, encoderCount)
						.Select(Converters.NativeToAudioEncoder)
						.ToList();
				}

				return audioEncoders;
			}
		}

		/// <summary>
		/// Gets a list of supported video encoders.
		/// </summary>
		public static List<HBVideoEncoder> VideoEncoders
		{
			get
			{
				if (videoEncoders == null)
				{
					IntPtr encodersPtr = HBFunctions.hb_get_video_encoders();
					int encoderCount = HBFunctions.hb_get_video_encoders_count();

					videoEncoders = InteropUtilities.ConvertArray<hb_encoder_s>(encodersPtr, encoderCount)
						.Select(Converters.NativeToVideoEncoder)
						.ToList();
				}

				return videoEncoders;
			}
		}

		/// <summary>
		/// Gets a list of supported mixdowns.
		/// </summary>
		public static List<HBMixdown> Mixdowns
		{
			get
			{
				if (mixdowns == null)
				{
					IntPtr mixdownsPtr = HBFunctions.hb_get_audio_mixdowns();
					int mixdownsCount = HBFunctions.hb_get_audio_mixdowns_count();

					mixdowns = InteropUtilities.ConvertArray<hb_mixdown_s>(mixdownsPtr, mixdownsCount)
						.Select(Converters.NativeToMixdown)
						.ToList();
				}

				return mixdowns;
			}
		}

		/// <summary>
		/// Gets a list of supported audio bitrates.
		/// </summary>
		public static List<int> AudioBitrates
		{
			get
			{
				if (audioBitrates == null)
				{
					IntPtr audioBitratesPtr = HBFunctions.hb_get_audio_bitrates();
					int audioBitratesCount = HBFunctions.hb_get_audio_bitrates_count();

					audioBitrates = InteropUtilities.ConvertArray<hb_rate_s>(audioBitratesPtr, audioBitratesCount)
						.Select(b => b.rate)
						.ToList();
				}

				return audioBitrates;
			}
		}

		/// <summary>
		/// Gets the audio encoder with the specified short name.
		/// </summary>
		/// <param name="shortName">The name of the audio encoder.</param>
		/// <returns>The requested audio encoder.</returns>
		public static HBAudioEncoder GetAudioEncoder(string shortName)
		{
			return AudioEncoders.SingleOrDefault(e => e.ShortName == shortName);
		}

		/// <summary>
		/// Gets the mixdown with the specified short name.
		/// </summary>
		/// <param name="shortName">The name of the mixdown.</param>
		/// <returns>The requested mixdown.</returns>
		public static HBMixdown GetMixdown(string shortName)
		{
			return Mixdowns.SingleOrDefault(m => m.ShortName == shortName);
		}

		/// <summary>
		/// Determines if the given encoder is compatible with the given track.
		/// </summary>
		/// <param name="track">The audio track to examine.</param>
		/// <param name="encoder">The encoder to examine.</param>
		/// <returns>True if the given encoder is comatible with the given audio track.</returns>
		/// <remarks>Only works with passthrough encoders.</remarks>
		public static bool AudioEncoderIsCompatible(AudioTrack track, HBAudioEncoder encoder)
		{
			return (track.CodecId & encoder.Id) > 0;
		}

		/// <summary>
		/// Finds the highest possible mixdown for a given audio encoder.
		/// </summary>
		/// <param name="audioEncoder">The audio encoder in question.</param>
		/// <returns>The highest possible mixdown for that audio encoder.</returns>
		public static int GetMaxMixdownIndex(HBAudioEncoder audioEncoder)
		{
			// To find best case scenario, pass in highest number of channels and 6-channel discrete mixdown.
			int maxMixdownId = HBFunctions.hb_get_best_mixdown((uint)audioEncoder.Id, NativeConstants.HB_INPUT_CH_LAYOUT_3F4R, NativeConstants.HB_AMIXDOWN_6CH);

			for (int i = 0; i < Mixdowns.Count; i++)
			{
				if (Mixdowns[i].Id == maxMixdownId)
				{
					return i;
				}
			}

			return -1;
		} 

		/// <summary>
		/// Sanitizes a mixdown given the output codec and input channel layout.
		/// </summary>
		/// <param name="mixdown">The desired mixdown.</param>
		/// <param name="encoder">The output encoder to be used.</param>
		/// <param name="layout">The input channel layout.</param>
		/// <returns>A sanitized mixdown value.</returns>
		public static HBMixdown SanitizeMixdown(HBMixdown mixdown, HBAudioEncoder encoder, int layout)
		{
			int sanitizedMixdown = HBFunctions.hb_get_best_mixdown((uint)encoder.Id, layout, mixdown.Id);
			return Mixdowns.Single(m => m.Id == sanitizedMixdown);
		}

		/// <summary>
		/// Gets the default mixdown for the given audio encoder and channel layout.
		/// </summary>
		/// <param name="encoder">The output codec to be used.</param>
		/// <param name="layout">The input channel layout.</param>
		/// <returns>The default mixdown for the given codec and channel layout.</returns>
		public static HBMixdown GetDefaultMixdown(HBAudioEncoder encoder, int layout)
		{
			int defaultMixdown = HBFunctions.hb_get_default_mixdown((uint)encoder.Id, layout);
			return Mixdowns.Single(m => m.Id == defaultMixdown);
		}

		/// <summary>
		/// Gets the bitrate limits for the given audio codec, sample rate and mixdown.
		/// </summary>
		/// <param name="encoder">The audio encoder used.</param>
		/// <param name="sampleRate">The sample rate used (Hz).</param>
		/// <param name="mixdown">The mixdown used.</param>
		/// <returns>Limits on the audio bitrate for the given settings.</returns>
		public static BitrateLimits GetBitrateLimits(HBAudioEncoder encoder, int sampleRate, HBMixdown mixdown)
		{
			int low = 0;
			int high = 0;

			HBFunctions.hb_get_audio_bitrate_limits((uint)encoder.Id, sampleRate, mixdown.Id, ref low, ref high);

			return new BitrateLimits { Low = low, High = high };
		}

		/// <summary>
		/// Sanitizes an audio bitrate given the output codec, sample rate and mixdown.
		/// </summary>
		/// <param name="audioBitrate">The desired audio bitrate.</param>
		/// <param name="encoder">The output encoder to be used.</param>
		/// <param name="sampleRate">The output sample rate to be used.</param>
		/// <param name="mixdown">The mixdown to be used.</param>
		/// <returns>A sanitized audio bitrate.</returns>
		public static int SanitizeAudioBitrate(int audioBitrate, HBAudioEncoder encoder, int sampleRate, HBMixdown mixdown)
		{
			return HBFunctions.hb_get_best_audio_bitrate((uint)encoder.Id, audioBitrate, sampleRate, mixdown.Id);
		}

		/// <summary>
		/// Gets the default audio bitrate for the given parameters.
		/// </summary>
		/// <param name="encoder">The encoder to use.</param>
		/// <param name="sampleRate">The sample rate to use.</param>
		/// <param name="mixdown">The mixdown to use.</param>
		/// <returns>The default bitrate for these parameters.</returns>
		public static int GetDefaultBitrate(HBAudioEncoder encoder, int sampleRate, HBMixdown mixdown)
		{
			return HBFunctions.hb_get_default_audio_bitrate((uint) encoder.Id, sampleRate, mixdown.Id);
		}

		/// <summary>
		/// Gets limits on audio quality for a given encoder.
		/// </summary>
		/// <param name="encoderId">The audio encoder ID.</param>
		/// <returns>Limits on the audio quality for the given encoder.</returns>
		internal static RangeLimits GetAudioQualityLimits(int encoderId)
		{
			float low = 0, high = 0, granularity = 0;
			int direction = 0;
			HBFunctions.hb_get_audio_quality_limits((uint)encoderId, ref low, ref high, ref granularity, ref direction);

			return new RangeLimits
			{
				Low = low,
				High = high,
				Granularity = granularity,
				Ascending = direction == 0
			};
		}

		/// <summary>
		/// Gets limits on audio compression for a given encoder.
		/// </summary>
		/// <param name="encoderId">The audio encoder ID.</param>
		/// <returns>Limits on the audio compression for the given encoder.</returns>
		internal static RangeLimits GetAudioCompressionLimits(int encoderId)
		{
			float low = 0, high = 0, granularity = 0;
			int direction = 0;
			HBFunctions.hb_get_audio_compression_limits((uint)encoderId, ref low, ref high, ref granularity, ref direction);

			return new RangeLimits
			{
				Low = low,
				High = high,
				Granularity = granularity,
				Ascending = direction == 0
			};
		}
	}
}
